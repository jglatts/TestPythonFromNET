import sys
import json
import cv2
import base64
import tempfile
from anomalib.data import PredictDataset
from anomalib.engine import Engine
from anomalib.models import Patchcore
import numpy as np

IMAGE_SCORE_THRESHOLD = 0.5
MIN_RADIUS = 3
CKPT_PATH = "model.ckpt"

# Initialize model & engine once
model = Patchcore()
engine = Engine()

def encode_image(img):
    _, buffer = cv2.imencode(".jpg", img)
    return base64.b64encode(buffer).decode("utf-8")


for line in sys.stdin:
    line = line.strip()
    if line.lower() == "exit":
        break

    # Expect frame path from C# (or could extend to receive base64)
    frame_path = line
    frame = cv2.imread(frame_path)
    if frame is None:
        print(json.dumps({"error": "cannot read frame"}), flush=True)
        continue

    # Save temp file for PredictDataset
    temp_file = tempfile.NamedTemporaryFile(suffix=".png", delete=False)
    cv2.imwrite(temp_file.name, frame)

    dataset = PredictDataset(path=temp_file.name, image_size=(512, 512))
    predictions = engine.predict(model=model, dataset=dataset, ckpt_path=CKPT_PATH)
    pred = predictions[0]

    # Anomaly map & score
    anomaly_map = pred.anomaly_map.squeeze().cpu().numpy()
    score = float(np.max(anomaly_map))
    status = "BAD" if score > IMAGE_SCORE_THRESHOLD else "GOOD"

    # Overlay
    heatmap = (255 * (anomaly_map - anomaly_map.min()) /
               (anomaly_map.max() - anomaly_map.min() + 1e-8)).astype(np.uint8)
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

    # Return JSON with base64 overlay
    output = {
        "status": status,
        "score": score,
        "overlay": encode_image(overlay)
    }
    print(json.dumps(output), flush=True)
