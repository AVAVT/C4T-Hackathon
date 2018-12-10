import json as j 
class Character:
    def __init__(self):
        self.x = None
        self.y = None
        self.team = None
        self.characterRole = 0
        self.harvest = 0
        self.isScared = False
        self.performAction1 = 0
        self.performAction2 = 0
        
    def do_start(self, json):
        game_state = j.loads(json)
        self.team = game_state['allies'][self.characterRole]['team']
        self.characterRole = game_state['allies'][self.characterRole]['characterRole']
        #method for preparing
        return "Ready"
    
    def do_turn(self, json):
        #this method will be called each turn!
        #this method has to return one of following values
        #corresponding with direction the character will go
        #"STAY", "DOWN", "UP", "LEFT", "RIGHT"
        #default return value is "STAY"
        #update attributes
        game_state = j.loads(json)
        self.x = game_state['allies'][self.characterRole]['x']
        self.y = game_state['allies'][self.characterRole]['y']
        self.harvest = game_state['allies'][self.characterRole]['harvest']
        self.isScared = game_state['allies'][self.characterRole]['isScared']
        self.performAction1 = game_state['allies'][self.characterRole]['performAction1']
        self.performAction2 = game_state['allies'][self.characterRole]['performAction2']
        
        return "STAY"
