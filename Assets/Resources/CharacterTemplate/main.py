import json as j 
class Character:
    def __init__(self):
        #Change this to 0: Grower, 1: Harvester, 2: Worm
        self.gameRule = None
        self.characterRole = 0
        self.x = None
        self.y = None
        self.team = None
        self.fruitCarrying = None
        self.isScared = False
        self.cancelAction = False

    def do_start(self, gameRule, json):
        #method for preparing
        self.gameRule = j.loads(gameRule)
        game_state = j.loads(json)
        self.team = game_state['allies'][self.characterRole]['team']
        return "READY"

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
        self.fruitCarrying = game_state['allies'][self.characterRole]['fruitCarrying']
        self.isScared = game_state['allies'][self.characterRole]['isScared']
        self.cancelAction = game_state['allies'][self.characterRole]['cancelAction']
        return "STAY"   
