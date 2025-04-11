# DICOM3DReconstructor

## Description

This project processes binary DICOM files (typically acquired through brain MRI scans), extracts 2D slices, performs contour detection, and reconstructs them into a 3D brain model using triangulation. The final model is exported as a `.obj` file, which can be viewed in any standard 3D model viewer.

The pipeline includes:
- Reading and rendering DICOM images.
- Extracting pixel spacing and patient position data for accurate 3D spatial transformation.
- Applying 2D contour detection on JPEG-converted slices.
- Transforming 2D contour coordinates into 3D space.
- Triangulating the contours and connecting layers to create a full 3D mesh.
- Saving the model in Wavefront `.obj` format.

This project was developed as part of a university program in collaboration with a software development company. The purpose of the project is to support the medical field by providing doctors with a clear 3D visualization of the brain based on MRI data. These visualizations help radiologists and other specialists more easily identify abnormal changes, improving both the accuracy and efficiency of medical diagnoses.

> **Note:** The repository does *not* include the source files for the contour detection module (`Contourdetection`), as that part was developed by other members of our 5-person team and provided to us externally.

The DICOM reader, 2D to 3D transformation, and 3D mesh reconstruction pipeline contained in this repository were implemented by me and one other teammate.
