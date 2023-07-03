# protoc --csharp_out=../C# --python_out=../Python main_proto.proto

import mediapipe_face_blendshapes 
import asyncio
import sys

IP = "localhost"  
PORT = 5004

async def main():
    blendShapesDetector = mediapipe_face_blendshapes.BlendShapesDetector
    blendShapesDetector.arg = sys.argv[1]
    server = await asyncio.start_server(blendShapesDetector, IP, int(sys.argv[2]))
    addr = server.sockets[0].getsockname()
    print(f'Serving on {addr}')
    async with server:
        await server.serve_forever()
    

if __name__ == '__main__':
    try:
        asyncio.run(main())
    except Exception as e:
        print(e)
        pass