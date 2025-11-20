"""
        Interface for Anomalib PatchCore (Anomalib 2.2)
            - Fully in-memory
            - No disk writes
            - Returns JSON with status, score, overlay (base64)
"""

import sys
import json
import cv2
import numpy as np
import base64
import torch
from torchvision import transforms
from anomalib.models import Patchcore

CKPT_PATH = "model.ckpt"
IMAGE_SCORE_THRESHOLD = 50
MIN_RADIUS = 3
INPUT_SIZE = (512, 512)

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
ran_from_shell = False
output = None


class SimpleBatch:
    def __init__(self, image):
        self.image = image
        self.label = None
        self.mask = None
        
    def update(self, **kwargs):
        for k, v in kwargs.items():
            setattr(self, k, v)
        return self



def encode_image(img: np.ndarray) -> str:
    _, buffer = cv2.imencode(".jpg", img)
    return base64.b64encode(buffer).decode("utf-8")


def analyze_anomaly_map(frame: np.ndarray, anomaly_map: np.ndarray):
    anomaly_map = np.array(anomaly_map)

    if anomaly_map.ndim == 0:
        anomaly_map = anomaly_map.reshape(1, 1)
    if anomaly_map.ndim == 1:
        anomaly_map = anomaly_map.reshape(1, -1)
    
    # compute the score from the anomaly map
    score = float(anomaly_map.max())
    status = "BAD" if score > IMAGE_SCORE_THRESHOLD else "GOOD"

    heatmap = (255 * (anomaly_map - anomaly_map.min()) /
               (np.ptp(anomaly_map) + 1e-8)).astype("uint8")

    heatmap = cv2.resize(heatmap, (frame.shape[1], frame.shape[0]))
    _, thresh = cv2.threshold(heatmap, 60, 255, cv2.THRESH_BINARY)
    contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    overlay = frame.copy()
    if status == "BAD":
        for cnt in contours:
            (x, y), radius = cv2.minEnclosingCircle(cnt)
            if radius > MIN_RADIUS:
                cv2.circle(overlay, (int(x), int(y)), int(radius), (0, 0, 255), 2)

    # Add RGB heatmap
    heatmap_color = cv2.applyColorMap(heatmap, cv2.COLORMAP_JET)
    overlay = cv2.addWeighted(overlay, 0.7, heatmap_color, 0.3, 0)

    return status, score, overlay


def process_frame(frame: np.ndarray, model: Patchcore):
    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    frame_resized = cv2.resize(frame_rgb, INPUT_SIZE)

    img_tensor = transforms.ToTensor()(frame_resized).unsqueeze(0).to(device)
    batch = SimpleBatch(img_tensor)
    with torch.no_grad():
        pred = model.predict_step(batch, batch_idx=0)

    anomaly_map = pred.anomaly_map.squeeze().cpu().numpy()
    return analyze_anomaly_map(frame, anomaly_map)


def extractFrame(path, model):
    frame = cv2.imread(path)

    if frame is None:
        output = {"error": f"Cannot read frame {path}"}
    else:
        status, score, overlay = process_frame(frame, model)
        output = {
            "status": status,
            "score": score
        }
        if not ran_from_shell:
            output["overlay"] = encode_image(overlay)
    
    return output


def main():
    print("Anomalib C# interface running (in-memory)", flush=True)

    # Load PatchCore once
    model = Patchcore.load_from_checkpoint(CKPT_PATH)
    model.eval()
    model.to(device)

    for line in sys.stdin:
        path = line.strip()
        if not path:
            print(json.dumps({"error": "Empty path"}), flush=True)
            continue
        if path.lower() == "exit":
            break

        global output
        output = extractFrame(path, model)
        print(json.dumps(output), flush=True)


def checkArgs():
    if len(sys.argv) > 1:
        global ran_from_shell
        ran_from_shell = True  
        print("got args")


if __name__ == "__main__":
    checkArgs()
    main()
    if ran_from_shell:
        with open("output.txt", "w") as f:
            f.write(json.dumps(output))
