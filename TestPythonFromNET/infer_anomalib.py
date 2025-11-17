"""
anomalib_infer.py

Minimal Python script to integrate Anomalib Patchcore with a C# frontend.

- Reads image paths from stdin.
- Runs Patchcore anomaly detection.
- Returns JSON with status, score, and overlay image (base64).
"""

import sys
import json
import cv2
import base64
import tempfile
import numpy as np
from anomalib.data import PredictDataset
from anomalib.engine import Engine
from anomalib.models import Patchcore

IMAGE_SCORE_THRESHOLD = 0.5
MIN_RADIUS = 3

CKPT_PATH = "model.ckpt"


def encode_image(img):
    _, buffer = cv2.imencode(".jpg", img)
    return base64.b64encode(buffer).decode("utf-8")


def analyze_prediction(pred, frame):
    """
    Analyze a Patchcore prediction and overlay anomalies on the frame.
    Returns status, max score, and overlay image.
    """
    anomaly_map = pred.anomaly_map.squeeze().cpu().numpy()
    image_score = float(anomaly_map.max())
    status = "BAD" if image_score > IMAGE_SCORE_THRESHOLD else "GOOD"

    # Create overlay
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
    overlay = cv2.addWeighted(overlay, 0.7, heatmap_color, 0.3, 0)

    return status, image_score, overlay


def predict_frame(frame):
    """
    Run Patchcore on a single OpenCV BGR frame.
    """
    model = Patchcore()
    engine = Engine()

    # Save frame temporarily for PredictDataset
    temp_file = tempfile.NamedTemporaryFile(suffix=".png", delete=False)
    cv2.imwrite(temp_file.name, frame)
    
    dataset = PredictDataset(path=temp_file.name, image_size=(512, 512))
    predictions = engine.predict(model=model, dataset=dataset, ckpt_path=CKPT_PATH)
    
    temp_file.close()
    
    return predictions


def process_image_path(path):
    frame = cv2.imread(path)
    if frame is None:
        return {"error": f"Cannot read frame {path}"}

    preds = predict_frame(frame)
    if preds:
        status, score, overlay = analyze_prediction(preds[0], frame)
    else:
        status, score, overlay = "GOOD", 0, frame

    return {
        "status": status,
        "score": score,
        "overlay": encode_image(overlay)
    }


def main():
    print("Anomalib C# interface running", flush=True)
    for line in sys.stdin:
        path = line.strip()
        if not path:
            continue
        if path.lower() == "exit":
            break

        output = process_image_path(path)
        print(json.dumps(output), flush=True)



main()
