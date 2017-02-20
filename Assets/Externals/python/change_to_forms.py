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
               "cup1",
               "cup2",
               "cup3",
               "cups",
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
                 "at",
                 "behind",
                 "in front of",
                 "near",
                 "left of",
                 "right of",
                 "center of",
                 "edge of",
                 "under",
                 "against"]

    attributes = ["brown",
                  "blue",
                  "black",
                  "green",
                  "yellow",
                  "red",
                  "leftmost",
                  "middle",
                  "rightmost"]

    determiners = ["the", "a", "two"]

    exclude = []

    s = filter(lambda a: a not in exclude, sent.split())

    #s = sent.split()

    form = s[0] + '('
    
    last_obj = ""
        
    i = 1
    while i < len(s):
        #print form,i
        if (i+2 < len(s) and s[i:i+3] == ['in', 'front', 'of']):
            form = form + ',in_front('
            s[i+1] = ""
            s[i+2] = ""
            i += 1
        elif (i+1 < len(s) and s[i:i+2] == ['left', 'of']):
            form = form + ',left('
            s[i+1] = ""
            i += 1
        elif (i+1 < len(s) and s[i:i+2] == ['right', 'of']):
            form = form + ',right('
            s[i+1] = ""
            i += 1
        elif (i+1 < len(s) and s[i:i+2] == ['center', 'of']):
            form = form + ',center('
            s[i+1] = ""
            i += 1
        elif s[i] in relations:
            if form[-1] == '(':
                form = form + s[i] + '('
            else:
                if s[i:i+2] == ['at','center']:
                    form = form + ',center' + '(' + last_obj
                elif s[i:i+2] == ['on','edge']:
                    form = form + ',edge' + '(' + last_obj
                else:
                    form = form + ',' + s[i] + '('
            i += 1
        elif s[i] in determiners:
            skip = 0
            if s[i-1] == "and":
                form = form + ',' + s[i] + '('
            else:
                form = form + s[i] + '('
            skip += 1
            for j in range(i+1,len(s)):
                if s[j] in attributes:
                    form = form + s[j] + '('
                    skip += 1
                elif (j+1 < len(s) and s[j:j+2] == ['paper', 'sheet']):
                    if s[j-1] == "and":
                        form = form + ',' + 'paper_sheet'
                    else:
                        form = form + 'paper_sheet'
                        for k in range(form.count('(')-form.count(')')-1):
                            form = form + ')'
                        s[j+1] = ""
                        skip += 1
                elif s[j] in objects:
                    last_obj = s[j]
                    if s[j-1] == "and":
                        form = form + ',' + s[j]
                        skip += 1
                    else:
                        form = form + s[j]
                        skip += 1
                elif s[j] != "and":
                    for k in range(form.count('(')-form.count(')')-1):
                        form = form + ')'
                    break
            i += skip
        elif s[i] in attributes:
            skip = 0
            if s[i-1] == "and":
                form = form + ',' + s[i] + '('
            else:
                form = form + s[i] + '('
            skip += 1
            for j in range(i+1,len(s)):
                if s[j] in attributes:
                    form = form + s[j] + '('
                    skip += 1
                elif (j+1 < len(s) and s[j:j+2] == ['paper', 'sheet']):
                    if s[j-1] == "and":
                        form = form + ',' + 'paper_sheet'
                    else:
                        form = form + 'paper_sheet'
                        for k in range(form.count('(')-form.count(')')-1):
                            form = form + ')'
                        s[j+1] = ""
                        skip += 1
                elif s[j] in objects:
                    last_obj = s[j]
                    if s[j-1] == "and":
                        form = form + ',' + s[j]
                        skip += 1
                    else:
                        form = form + s[j]
                        skip += 1
                elif s[j] != "and":
                    for k in range(form.count('(')-form.count(')')-1):
                        form = form + ')'
                    break
            i += skip
        elif (i+1 < len(s) and s[i:i+2] == ['paper', 'sheet']):
            if s[i-1] == "and":
                form = form + ',' + 'paper_sheet'
            else:
                form = form + 'paper_sheet'
            for k in range(form.count('(')-form.count(')')-1):
                form = form + ')'
            s[i+1] = ""
            i += 1
        elif s[i] in objects:
            last_obj = s[i]
            if s[i-1] == "and":
                form = form + ',' + s[i]
            else:
                form = form + s[i]
            for k in range(form.count('(')-form.count(')')-1):
                form = form + ')'
            i += 1
        else:
            i += 1

    for i in range(form.count('(')-form.count(')')):
        form = form + ')'

    return form
