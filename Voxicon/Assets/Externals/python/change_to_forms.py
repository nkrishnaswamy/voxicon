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
               "blocks",
               "lid",
               "stack"]
  
    relations = ["touching",
                 "in",
                 "on",
                 "behind",
                 "in front of",
                 "near",
                 "left of",
                 "right of"]
      
    s = sent.split()
    form = s[0] + '('
    for i in range(1,len(s)):
        if s[i] in relations:
            form = form + ',' + s[i] + '('
        elif (i+2 < len(s)):
            if (s[i] == 'in' and s[i+1] == 'front' and s[i+2] == 'of'):
                form = form + ',in_front_of('
        elif (i+1 < len(s)):
            if s[i] == 'left' and s[i+1] == 'of':
                form = form + ',left_of('
        elif (s[i] == 'right' and s[i+1] == 'of'):
            form = form + ',right_of('
        elif s[i] == 'paper' and s[i+1] == 'sheet':
            form = form + 'paper_sheet'
                                  
        if s[i] in objects:
            form = form + s[i]
        elif s[i] == 'edge':
            form = form + 'edge'
        elif s[i] == 'center':
            form = form + 'center'

    form = form + ')'
    if ',' in form:
        form = form + ')'

    return form