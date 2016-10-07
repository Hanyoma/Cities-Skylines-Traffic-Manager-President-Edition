import socket
import json
import logging
import argparse

logging.basicConfig(level=logging.INFO)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send(data_string):
    sock.settimeout(1)
    print 'Send', data_string, 'to simulator'
    #sock.sendto(data_string, ("192.168.0.112",11000))
    sock.sendto(data_string, ("localhost", 11000))

    try:
        response, srvr = sock.recvfrom(1024)
        print "response from", srvr, "is", response
    except socket.timeout:
        response = ""
        logging.warning('Request timed out')
        return response

def getState():
    data = {
    'Method': 'GETSTATE',
    'Object': {
                'Name': 'NodeId',
                'Type': 'PARAMETER',
                'Value': 0,  #// should be 0 - 3 (for the selected ids)
                'ValueType': 'System.UInt32'
                }
    }
    logging.info('data from getState is: %s', data)
    return data;

def jsonString(data):
    json_string = json.dumps(data)
    logging.info('jsonString: %s', json_string)
    return json_string;


response = send(jsonString(getState()))
