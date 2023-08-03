import cv2
import sys
import Server
import asyncio
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from mvm_pb2 import DataChannelMessage,DataChannelMessageType

mp_face_mesh = mp.solutions.face_mesh

async def BlendShapesDetector (reader,writer) :
    try:
        base_options = python.BaseOptions(model_asset_path='./Assets/Scripts/Python/model/face_landmarker.task')
        options = vision.FaceLandmarkerOptions(base_options=base_options,
                                            output_face_blendshapes=True,
                                            output_facial_transformation_matrixes=True,
                                            num_faces=1)
        blendShapesDetector = vision.FaceLandmarker.create_from_options(options)
        face_mesh = mp.solutions.face_mesh.FaceMesh(
            max_num_faces=1,
            refine_landmarks=True,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5)
        
        cap = cv2.VideoCapture(int(sys.argv[1]))

        while cap.isOpened():
            md_blendShapes = None
            md_keypoints = None
            success, image = cap.read()
            # BlendShapes 
            image_1 = mp.Image(image_format=mp.ImageFormat.SRGB, data=image)
            blendShapes_detection_result = blendShapesDetector.detect(image_1)
            if (len(blendShapes_detection_result.face_blendshapes) > 0) : 
                # Pack the serialized data and send it over the socket
                md_blendShapes = blendShapes_detection_result.face_blendshapes[0]
            
            # Keypoints 
            image.flags.writeable = False
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = face_mesh.process(image)
            if (len(results.multi_face_landmarks) > 0) :
                md_keypoints = results.multi_face_landmarks[0].landmark
    
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