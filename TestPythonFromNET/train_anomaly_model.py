"""
=====================================================================
Anomalib Training Script for MVTecAD and Custom Folder Datasets
=====================================================================

Overview:
    This script provides a simple wrapper for training and testing
    anomaly-detection models using Anomalib. It supports both:
        • Custom folder-based datasets (normal/abnormal directory structure)
        • Full MVTecAD-formatted datasets

    The CreateEngine class encapsulates dataset construction, model
    initialization (PatchCore), and execution of training/testing cycles.

Features:
    • Auto train/test split for folder datasets
    • MVTecAD-compliant datamodule option
    • PatchCore model with configurable nearest-neighbor count
    • Standardized Engine lifecycle (fit → test)

Notes:
    • For Windows users: run this script from an elevated(Admin) terminal
      to avoid permission issues when Anomalib writes logs/checkpoints.
    • Folder datasets must follow "normal_dir / abnormal_dir" structure.
    • MVTecAD datasets must follow official directory conventions (see docs).

Author:
    John Glatts
    Z-Axis Connector Company
    Date: 11/19/2025

"""
from anomalib.data import MVTecAD
from anomalib.data import Folder
from anomalib.engine import Engine
from anomalib.models import Patchcore


class CreateEngine:
    def __init__(self, dataset_root="", category_name="", 
                 normal_dir="", bad_dir=""):
        self.dataset_root = dataset_root
        self.category_name = category_name
        self.normal_dir = normal_dir
        self.bad_dir = bad_dir
        self.engine = None
        self.model = None
        self.datamodule = None
        self.is_created = False

    def trainEngine(self):
        if not self.is_created:
            return

        print("\n\n🚀 Starting Training... 🚀\n\n")
        self.engine.fit(datamodule=self.datamodule, model=self.model)
        print("\n\n🚀 Training Complete... 🚀\n\n")

    def testEngine(self):
        if not self.is_created:
            return

        print("\n\n🚀 Starting Testing... 🚀\n")

        # Run test and capture metrics
        results_list = self.engine.test(datamodule=self.datamodule, model=self.model)

        print("\n📊 Test Metrics Explained:\n")

        # Define padding width
        label_width = 20

        print(f"{'Metric'.ljust(label_width)} : {'Value'.ljust(10)} : Explanation")
        print("-" * 80)

        # Loop over results (list of dicts)
        for i, results in enumerate(results_list):
            auroc = results.get("image_AUROC", None)
            f1 = results.get("image_F1Score", None)

            print(f"[DataLoader {i}]")
            print(f"{'Image AUROC'.ljust(label_width)} : {auroc:<10.4f} : "
                  "AUROC (Area Under the ROC Curve) measures the model's ability "
                  "to distinguish between normal and anomalous images. "
                  "1.0 = perfect separation, 0.5 = random guessing.")
            print(f"{'Image F1 Score'.ljust(label_width)} : {f1:<10.4f} : "
                  "F1 Score is the harmonic mean of precision and recall for detecting anomalies "
                  "at the image level. Higher is better (good balance between detecting anomalies and avoiding false alarms).")
            print("-" * 80)

        print("\n🚀 Testing Complete... 🚀\n")


    def createEngineSimple(self):
        self.datamodule = Folder(
            name=self.category_name + "_dataset",
            root=self.dataset_root,
            normal_dir=self.normal_dir,       # where the normal samples are
            abnormal_dir=self.bad_dir,        # where the defects are
            normal_split_ratio=0.8,           # auto split
        )

        self.model = Patchcore(num_neighbors=6)
        self.engine = Engine(max_epochs=10)          # for smaller dataset, use more epochs
        self.is_created = True

        # Train
        self.trainEngine()

        # Test
        self.testEngine()

    # NOTE
    #   datasets must follow MVTecAD structure
    def createEngineMVTecAD(self):
        # Create dataset
        self.datamodule = MVTecAD(
            root=self.dataset_root,
            category=self.category_name + "_datset",
            train_batch_size=8,
            eval_batch_size=8,
        )
        
        # Initialize model and engine
        self.model = Patchcore(num_neighbors=6)
        self.engine = Engine(max_epochs=5)
        self.is_created = True

        # Train
        self.trainEngine()

        # Test
        self.testEngine()


if __name__ == "__main__":
    # NOTE
    #   normal and bad directories are relative to dataset_root
    dataset_root = r"C:\Users\jglatts\Documents\Z-Axis\YOLO-CV\datasets\Z-Axis"
    newEngine = CreateEngine(dataset_root=dataset_root, 
                             category_name="z-fill",
                             normal_dir="zfill/train/good",
                             bad_dir="zfill/test/bad")

    newEngine.createEngineSimple()