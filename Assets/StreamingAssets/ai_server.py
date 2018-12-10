from concurrent import futures
import AI_Action_pb2
import AI_Action_pb2_grpc
import grpc
import json as j
import time

from blue_planter.main import Character as blue_planter
from blue_harvester.main import Character as blue_harvester
from blue_worm.main import Character as blue_worm
from red_planter.main import Character as red_planter
from red_harvester.main import Character as red_harvester
from red_worm.main import Character as red_worm

class AIServiceServicer(AI_Action_pb2_grpc.AIServiceServicer):
  def __init__(self):
    self.port = 50051
    self.server = grpc.server(futures.ThreadPoolExecutor(max_workers=7))
    self.bp = blue_planter()
    self.bh = blue_harvester()
    self.bw = blue_worm()
    self.rp = red_planter()
    self.rh = red_harvester()
    self.rw = red_worm()

  def ReturnAIResponse(self, request, context):
    """Get map info then return AI response
    """
    jsonObject = j.loads(request.json)
    if jsonObject['turn'] == 0:
      action = self.call_character_dostart(request.index, request.json)
    else:
      action = self.call_character_doturn(request.index, request.json)
    return AI_Action_pb2.AIResponse(action=action)
  
  def call_character_dostart(self, index, json):
    if index==0: return self.rp.do_start(json)
    elif index==1: return self.rh.do_start(json)
    elif index==2: return self.rw.do_start(json)
    elif index==3: return self.bp.do_start(json)
    elif index==4: return self.bh.do_start(json)
    elif index==5: return self.bw.do_start(json)
    else: raise Exception("Invalid character index!")
  
  def call_character_doturn(self, index, json):
    if index==0: return self.rp.do_turn(json)
    elif index==1: return self.rh.do_turn(json)
    elif index==2: return self.rw.do_turn(json)
    elif index==3: return self.bp.do_turn(json)
    elif index==4: return self.bh.do_turn(json)
    elif index==5: return self.bw.do_turn(json)
    else: raise Exception("Invalid character index!")

  def ShutdownServer(self, request, context):
    print('Shutting down server!')
    AI_Action_pb2_grpc.add_AIServiceServicer_to_server(AIServiceServicer(), self.server)
    self.server.stop(0)
    print('Press Ctrl-C to quit!')
    return AI_Action_pb2.ShutdownResponse(status = "Shutted down!")

  def start_server(self):
    AI_Action_pb2_grpc.add_AIServiceServicer_to_server(AIServiceServicer(), self.server)
    self.server.add_insecure_port('[::]:50051')
    self.server.start()
    self.running = True
    print('Server is listening on port: 50051')
    try:
      while True:
        time.sleep(60 * 60)
    except KeyboardInterrupt:
      self.server.stop(0)

if __name__ == '__main__':
  service = AIServiceServicer()
  service.start_server()