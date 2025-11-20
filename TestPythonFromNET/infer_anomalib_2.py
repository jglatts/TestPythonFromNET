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


class SimpleBatch:
    def __init__(self, image):
        self.image = image
        self.label = None
        self.mask = None
        
    def update(self, **kwargs):
        for k, v in kwargs.items():
            setattr(self, k, v)
        return self


class Detector:
    def __init__(self):
        self.CKPT_PATH = "model.ckpt"
        self.IMAGE_SCORE_THRESHOLD = 50
        self.MIN_RADIUS = 3
        self.INPUT_SIZE = (512, 512)
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        self.ran_from_shell = False
        self.output = None

    def encode_image(self, img):
        _, buffer = cv2.imencode(".jpg", img)
        return base64.b64encode(buffer).decode("utf-8")


    def analyze_anomaly_map(self, frame, anomaly_map):
        anomaly_map = np.array(anomaly_map)

        if anomaly_map.ndim == 0:
            anomaly_map = anomaly_map.reshape(1, 1)
        if anomaly_map.ndim == 1:
            anomaly_map = anomaly_map.reshape(1, -1)
    
        # compute the score from the anomaly map
        score = float(anomaly_map.max())
        status = "BAD" if score > self.IMAGE_SCORE_THRESHOLD else "GOOD"

        heatmap = (255 * (anomaly_map - anomaly_map.min()) /
                   (np.ptp(anomaly_map) + 1e-8)).astype("uint8")

        heatmap = cv2.resize(heatmap, (frame.shape[1], frame.shape[0]))
        _, thresh = cv2.threshold(heatmap, 60, 255, cv2.THRESH_BINARY)
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

        overlay = frame.copy()
        if status == "BAD":
            for cnt in contours:
                (x, y), radius = cv2.minEnclosingCircle(cnt)
                if radius > self.MIN_RADIUS:
                    cv2.circle(overlay, (int(x), int(y)), int(radius), (0, 0, 255), 2)

        # Add RGB heatmap
        heatmap_color = cv2.applyColorMap(heatmap, cv2.COLORMAP_JET)
        overlay = cv2.addWeighted(overlay, 0.7, heatmap_color, 0.3, 0)

        return status, score, overlay


    def process_frame(self, frame, model):
        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        frame_resized = cv2.resize(frame_rgb, self.INPUT_SIZE)

        img_tensor = transforms.ToTensor()(frame_resized).unsqueeze(0).to(self.device)
        batch = SimpleBatch(img_tensor)
        with torch.no_grad():
            pred = model.predict_step(batch, batch_idx=0)

        anomaly_map = pred.anomaly_map.squeeze().cpu().numpy()
        return self.analyze_anomaly_map(frame, anomaly_map)


    def extractFrame(self, path, model):
        frame = cv2.imread(path)

        if frame is None:
            output = {"error": f"Cannot read frame {path}"}
        else:
            status, score, overlay = self.process_frame(frame, model)
            output = {
                "status": status,
                "score": score
            }
            if not self.ran_from_shell:
                output["overlay"] = self.encode_image(overlay)
    
        return output


    def main(self):
        print("Anomalib C# interface running (in-memory)", flush=True)

        # Load PatchCore once
        model = Patchcore.load_from_checkpoint(self.CKPT_PATH)
        model.eval()
        model.to(self.device)

        for line in sys.stdin:
            path = line.strip()
            if not path:
                print(json.dumps({"error": "Empty path"}), flush=True)
                continue
            if path.lower() == "exit":
                break

            self.output = self.extractFrame(path, model)
            print(json.dumps(self.output), flush=True)


    def checkArgs(self):
        if len(sys.argv) > 1:
            self.ran_from_shell = True  
            print("got args")
    

    def run(self):
        self.checkArgs()
        self.main()
        if self.ran_from_shell:
            with open("output.txt", "w") as f:
                f.write(json.dumps(self.output))



if __name__ == "__main__":
    detector = Detector()
    detector.run()