def parse_sent(sent):
    events = ["grasp",
              "hold",
              "touch",
              "move",
              "turn",
              "roll",
              "spin",
              "stack",
              "put",
              "lean on",
              "lean against",
              "flip on edge",
              "flip at center",
              "flip",
              "close",
              "open",
              "lift",
              "drop",
              "reach",
              "slide"]
        
    objects = ["block",
               "ball",
               "plate",
               "cup",
               "disc",
               "spoon",
               "book",
               "blackboard",
               "bottle",
               "grape",
               "apple",
               "banana",
               "table",
               "bowl",
               "knife",
               "pencil",
               "paper sheet",
               "hand",
               "arm",
               "mug",
               "block1",
               "block2",
               "block3",
               "block4",
               "block5",
               "block6",
               "blocks",
               "lid",
               "stack",
               "staircase",
               "pyramid"]
  
    relations = ["touching",
                 "in",
                 "on",
                 "behind",
                 "in front of",
                 "near",
                 "left of",
                 "right of"]
                 
    attributes = ["brown",
                  "blue",
                  "black",
                  "green",
                  "yellow",
                  "red"]
                  
    determiners = ["a",
                   "the"]
      
    s = filter(lambda a: a not in determiners, sent.split())
    
    form = s[0] + '('
    
    for i in range(1,len(s)):
        if (i+2 < len(s) and s[i:i+3] == ['in', 'front', 'of']):
            form = form + ',in_front('
            s[i+1] = ""
            s[i+2] = ""
        elif (i+1 < len(s) and s[i:i+2] == ['left', 'of']):
            form = form + ',left('
            s[i+1] = ""
        elif (i+1 < len(s) and s[i:i+2] == ['right', 'of']):
            form = form + ',right('
            s[i+1] = ""
        elif (i+1 < len(s) and s[i:i+2] == ['paper', 'sheet']):
            form = form + 'paper_sheet'
            s[i+1] = ""
        elif s[i] in relations:
            if form[-1] == '(':
                form = form + s[i] + '('
            else:
                form = form + ',' + s[i] + '('
        elif s[i] in attributes:
            form = form + s[i] + '(' + s[i+1] + ')'
            s[i+1] = ""
        elif s[i] in objects:
            form = form + s[i]
    
        if s[i] == 'edge':
            form = form + 'edge'

        if s[i] == 'center':
            form = form + 'center'

    for i in range(form.count('(')-form.count(')')):
        form = form + ')'
        #if ',' in form:
        #form = form + ')'


    return form