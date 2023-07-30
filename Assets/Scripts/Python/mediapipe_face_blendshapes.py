import cv2
import sys
import Server
import asyncio
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from mvm_pb2 import BlendShapes,PythonServerMessage

async def BlendShapesDetector (reader,writer) :
    try:
        base_options = python.BaseOptions(model_asset_path='./Assets/Scripts/Python/model/face_landmarker.task')
        options = vision.FaceLandmarkerOptions(base_options=base_options,
                                            output_face_blendshapes=True,
                                            output_facial_transformation_matrixes=True,
                                            num_faces=1)
        detector = vision.FaceLandmarker.create_from_options(options)
        cap = cv2.VideoCapture(int(sys.argv[1]))
        while cap.isOpened():
            success, image = cap.read()
            image = mp.Image(image_format=mp.ImageFormat.SRGB, data=image)
            detection_result = detector.detect(image)
            if (len(detection_result.face_blendshapes) > 0) : 
                # Pack the serialized data and send it over the socket
                md_blendShapes = encodeBlendShapes(detection_result.face_blendshapes[0]).SerializeToString()
                writer.write(md_blendShapes)
                await asyncio.sleep(0.04) # wait for 33ms (30fps)
    except Exception as e: 
        print(e)
        Server.SendError("ERROR FOUND")



def encodeBlendShapes(blendShapes):
    message = PythonServerMessage()
    for element in blendShapes :
        blendShape = message.blend_shapes.blend_shapes.add()
        blendShape.index = element.index
        blendShape.score = element.score
        blendShape.category_name = element.category_name
    return message
