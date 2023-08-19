import cv2
import sys
import Server
import asyncio
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from mvm_pb2 import DataChannelMessage,DataChannelMessageType


async def BlendShapesDetector (reader,writer) :
    try:
        print(sys.argv)
        base_options = python.BaseOptions(model_asset_path=sys.argv[3] + '/StreamingAssets/Python/model/face_landmarker.task')
        options = vision.FaceLandmarkerOptions(base_options=base_options,
                                            output_face_blendshapes=True,
                                            output_facial_transformation_matrixes=True,
                                            num_faces=1)
        detector = vision.FaceLandmarker.create_from_options(options)
        
        cap = cv2.VideoCapture(int(sys.argv[1]))

        while cap.isOpened():
            md_blendShapes = None
            md_keypoints = None
            success, image = cap.read()
            # BlendShapes 
            image = mp.Image(image_format=mp.ImageFormat.SRGB, data=image)
            detection_result = detector.detect(image)
            if (len(detection_result.face_blendshapes) > 0) : 
                # Pack the serialized data and send it over the socket
                md_blendShapes = detection_result.face_blendshapes[0]
            
            if (len(detection_result.face_landmarks) > 0) :
                md_keypoints = detection_result.face_landmarks[0]
            # Encoding tracking message
            trackingMessage = encodeTrackingMessage(md_blendShapes , md_keypoints).SerializeToString()
            writer.write(trackingMessage)
            await asyncio.sleep(0.05) # wait for 33ms (30fps)

    except Exception as e: 
        print(e)
        # Server.SendError("ERROR FOUND")


def encodeTrackingMessage(blendShapes,keypoints):
    message = DataChannelMessage()
    message.type = DataChannelMessageType.TRACKING_MESSAGE
    if blendShapes is not None and len(blendShapes) > 0 :
        for element in blendShapes :
            blendShape = message.tracking_message.blendShapes.blendShapes.add()
            blendShape.index = element.index
            blendShape.score = element.score
            blendShape.category_name = element.category_name
    if keypoints is not None and len(keypoints) > 0 :
        for landmark in keypoints :
            keypoint = message.tracking_message.keypoints.keypoints.add()
            keypoint.x = landmark.x
            keypoint.y = landmark.y
            keypoint.z = landmark.z

    return message