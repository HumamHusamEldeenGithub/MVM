# protoc --csharp_out=../C# --python_out=../Python main_proto.proto

import socket
import sys
from main_proto_pb2 import Keypoint

IP = "localhost"  
PORT = 5000  

def RunServer() :
    global server
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((IP, PORT))
    server.listen()
    print(f"Server started at IP {IP} and PORT {PORT}")

def WaitForSocketConnection():
    global client ,address
    client, address = server.accept()
    print(f"Connection from {address} has been established.")

def SendKeyPoint(x,y) :
    message = Keypoint()
    message.x = x
    message.y = y
    client.send(message.SerializeToString())
    print(f"Sent: X = {message.x} Y = {message.y}")

def main():
    RunServer()
    WaitForSocketConnection()
    SendKeyPoint(10,20)

if __name__ == '__main__':
    print(sys.executable)
    main()