"""
    anomalib_infer.py

    Real-time Patchcore anomaly detection for continuous images.
    Returns JSON with status, score, and overlay image (base64).
    No disk writes.
"""
import sys
import json
import cv2
import base64
import numpy as np
import torch
from torchvision import transforms
from anomalib.models import Patchcore

CKPT_PATH = "model.ckpt"
THRESHOLD = 0.5
MIN_RADIUS = 3
INPUT_SIZE = (512, 512)

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
model = Patchcore.load_from_checkpoint(CKPT_PATH)
model.eval()
model.to(device)


def encode_image(img: np.ndarray) -> str:
    _, buffer = cv2.imencode(".jpg", img)
    return base64.b64encode(buffer).decode("utf-8")


def predict_frame(frame: np.ndarray) -> np.ndarray:
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    rgb_resized = cv2.resize(rgb, INPUT_SIZE)
    img_tensor = transforms.ToTensor()(rgb_resized).unsqueeze(0).to(device)

    with torch.no_grad():
        output = model(img_tensor)

    # Handle tuple or tensor output
    anomaly_map_tensor = output[0] if isinstance(output, tuple) else output
    anomaly_map_np = anomaly_map_tensor.squeeze().cpu().numpy()

    # Ensure 2D: only collapse extra dims if needed
    if anomaly_map_np.ndim == 0:
        # single scalar, expand to 2D
        anomaly_map_np = np.full(INPUT_SIZE, anomaly_map_np.item(), dtype=np.float32)
    elif anomaly_map_np.ndim == 3:
        # multiple channels, average
        anomaly_map_np = np.mean(anomaly_map_np, axis=0)
    elif anomaly_map_np.ndim != 2:
        raise ValueError(f"Unexpected anomaly_map shape: {anomaly_map_np.shape}")

    return anomaly_map_np


def analyze_prediction(anomaly_map: np.ndarray, frame: np.ndarray):
    score = float(anomaly_map.max())
    status = "BAD" if score > THRESHOLD else "GOOD"

    heatmap = (255 * (anomaly_map - anomaly_map.min()) / (np.ptp(anomaly_map) + 1e-8)).astype("uint8")
    heatmap = cv2.resize(heatmap, (frame.shape[1], frame.shape[0]))

    _, thresh = cv2.threshold(heatmap, 60, 255, cv2.THRESH_BINARY)
    contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    overlay = frame.copy()
    if status == "BAD":
        for cnt in contours:
            (x, y), radius = cv2.minEnclosingCircle(cnt)
            if radius > MIN_RADIUS:
                cv2.circle(overlay, (int(x), int(y)), int(radius), (0, 0, 255), 2)

    heatmap_color = cv2.applyColorMap(heatmap, cv2.COLORMAP_JET)
    overlay = cv2.addWeighted(frame, 0.3, heatmap_color, 0.7, 0)
    return status, score, overlay


def process_frame(frame: np.ndarray) -> dict:
    anomaly_map = predict_frame(frame)
    status, score, overlay = analyze_prediction(anomaly_map, frame)
    return {"status": status, "score": score, "overlay": encode_image(overlay)}


def main():
    print("Anomalib C# interface running (real-time, in-memory)", flush=True)
    for line in sys.stdin:
        path = line.strip()
        if not path:
            continue
        if path.lower() == "exit":
            break

        frame = cv2.imread(path)
        if frame is None:
            output = {"error": f"Cannot read frame {path}"}
        else:
            output = process_frame(frame)

        print(json.dumps(output), flush=True)



if __name__ == "__main__":
    main()
