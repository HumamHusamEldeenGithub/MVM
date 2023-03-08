import mediapipe as mp
import cv2
import Server
import asyncio
import sys
from main_proto_pb2 import Keypoints

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_face_mesh = mp.solutions.face_mesh

async def FacemeshDetector(reader,writer):
    try:
        drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
        cap = cv2.VideoCapture(int(sys.argv[1]))
        with mp_face_mesh.FaceMesh(
            max_num_faces=1,
            refine_landmarks=True,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5) as face_mesh:
            while cap.isOpened():
                success, image = cap.read()
                if not success:
                    print("Ignoring empty camera frame.")
                    # If loading a video, use 'break' instead of 'continue'.
                    continue

                # To improve performance, optionally mark the image as not writeable to
                # pass by reference.
                image.flags.writeable = False
                image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
                results = face_mesh.process(image)

                # Draw the face mesh annotations on the image.
                image.flags.writeable = True
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
                if results.multi_face_landmarks:
                    for face_landmarks in results.multi_face_landmarks:
                        if (len(face_landmarks.landmark) > 0) : 
                            # Pack the serialized data and send it over the socket
                            md_keypoints = encodeLandmarks(face_landmarks.landmark).SerializeToString()
                            writer.write(md_keypoints)
                            await asyncio.sleep(0.01) # wait for 33ms (30fps)

                        mp_drawing.draw_landmarks(
                            image=image,
                            landmark_list=face_landmarks,
                            connections=mp_face_mesh.FACEMESH_TESSELATION,
                            landmark_drawing_spec=None,
                            connection_drawing_spec=mp_drawing_styles
                            .get_default_face_mesh_tesselation_style())
                        mp_drawing.draw_landmarks(
                            image=image,
                            landmark_list=face_landmarks,
                            connections=mp_face_mesh.FACEMESH_CONTOURS,
                            landmark_drawing_spec=None,
                            connection_drawing_spec=mp_drawing_styles
                            .get_default_face_mesh_contours_style())
                        mp_drawing.draw_landmarks(
                            image=image,
                            landmark_list=face_landmarks,
                            connections=mp_face_mesh.FACEMESH_IRISES,
                            landmark_drawing_spec=None,
                            connection_drawing_spec=mp_drawing_styles
                            .get_default_face_mesh_iris_connections_style())
                        # # Flip the image horizontally for a selfie-view display.
                        cv2.imshow('MediaPipe Face Mesh', cv2.flip(image, 1))
                        if cv2.waitKey(1) == 27:
                            break
        cap.release()
    except Exception as e: 
        print(e)
        Server.SendError("ERROR FOUND")

def encodeLandmarks(landmarks):
    keypoints = Keypoints()
    for landmark in landmarks :
        keypoint = keypoints.points.add()
        keypoint.x = landmark.x
        keypoint.y = landmark.y
        keypoint.z = landmark.z
    return keypoints
