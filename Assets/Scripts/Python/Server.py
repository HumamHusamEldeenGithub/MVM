# protoc --csharp_out=../C# --python_out=../Python main_proto.proto

import mediapipe_facemesh
import asyncio

IP = "localhost"  
PORT = 5004

async def main():
    server = await asyncio.start_server(mediapipe_facemesh.FacemeshDetector, IP, PORT)
    addr = server.sockets[0].getsockname()
    print(f'Serving on {addr}')
    async with server:
        await server.serve_forever()
    

if __name__ == '__main__':
    try:
        asyncio.run(main())
    except :
        pass