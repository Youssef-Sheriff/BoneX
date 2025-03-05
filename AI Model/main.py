import os
try:
    import gdown
except ImportError:
    os.system('pip install gdown')
    import gdown

from flask import Flask, request, jsonify
try:
    from flask_cors import CORS
except ImportError:
    os.system('pip install flask-cors')
    from flask_cors import CORS
    
import keras
import numpy as np
import cv2
from PIL import Image
import io

app = Flask(__name__)
CORS(app)  # Enable CORS for all routes

# Google Drive File ID (Extracted from your link)
MODEL_ID = "1ZwS3XGujLt2z6XoaDJ_7DTW7UG-bMA3m"
MODEL_PATH = "BoneX_Final_ModelV4.h5"

# Function to download model from Google Drive
def download_model():
    if not os.path.exists(MODEL_PATH):  # Download only if not present
        print("Downloading model from Google Drive...")
        gdown.download(f"https://drive.google.com/uc?id={MODEL_ID}", MODEL_PATH, quiet=False)
        print("Model downloaded successfully!")

# Ensure model is downloaded before loading
download_model()

# Load BoneX model
model = keras.models.load_model(MODEL_PATH)

# Preprocessing function
def preprocess(image):
    image = image.convert("L")  # Convert to grayscale
    image = image.resize((224, 224))  # Resize to model input shape
    image = np.array(image, dtype=np.uint8)

    # Apply CLAHE
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    image = clahe.apply(image)

    # Normalize to [0, 1]
    image = (image - image.min()) / (image.max() - image.min())

    # Expand dimensions for model input
    image = np.expand_dims(image, axis=-1)  # Add channel dimension
    image = np.expand_dims(image, axis=0)  # Add batch dimension
    return image

# API route to receive image and predict
@app.route('/predict', methods=['POST'])
def predict():
    if 'file' not in request.files:
        return jsonify({'error': 'No file provided'}), 400

    file = request.files['file']
    image = Image.open(io.BytesIO(file.read()))  # Read image

    # Preprocess and predict
    processed_image = preprocess(image)
    prediction = model.predict(processed_image)
    result = int(prediction[0][0] > 0.5)  # Convert to 0 or 1
    result_text = "There's a fracture!" if result == 0 else "No fracture detected!"

    return jsonify({'prediction': result_text})

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=True, port=5000)