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
    def __init__(self, dataset_root="", category_name="", normal_dir="", bad_dir=""):
        self.dataset_root = dataset_root
        self.category_name = category_name
        self.normal_dir = normal_dir
        self.bad_dir = bad_dir

    def createEngineSimple(self):
        datamodule = Folder(
            name=self.category_name + "_dataset",
            root=self.dataset_root,
            normal_dir=self.normal_dir,       # where the normal samples are
            abnormal_dir=self.bad_dir,        # where the defects are
            normal_split_ratio=0.8,           # auto split
        )

        model = Patchcore(num_neighbors=6)
        engine = Engine(max_epochs=10)          # for smaller dataset, use more epochs

        # Train
        engine.fit(datamodule=datamodule, model=model)
        
        # Test
        engine.test(datamodule=datamodule, model=model)


    # NOTE
    #   datasets must follow MVTecAD structure
    def createEngineMVTecAD(self):
        # Create dataset
        datamodule = MVTecAD(
            root=self.dataset_root,
            category=self.category_name + "_datset",
            train_batch_size=8,
            eval_batch_size=8,
        )

        # Initialize model and engine
        model = Patchcore(num_neighbors=6)
        engine = Engine(max_epochs=5)

        # Train
        engine.fit(datamodule=datamodule, model=model)
        
        # Test
        engine.test(datamodule=datamodule, model=model)



if __name__ == "__main__":
    # NOTE
    #   normal and bad directories are relative to dataset_root
    newEngine = CreateEngine(dataset_root="./datasets/Z-Axis", 
                             category_name="z-fill",
                             normal_dir="zfill/train/good",
                             bad_dir="zfill/test/bad")

    newEngine.createEngineSimple(normal_dir="good", bad_dir="bad")