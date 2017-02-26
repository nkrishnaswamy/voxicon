import os

def send_next_event_to_port(index, port):
    sentence_list = ["put the apple on the plate",
                     "put the plate on the cup"]
                     
    i = int(index)
    os.system('''echo "%s" | nc -w 0 localhost %s''' % (sentence_list[i], port))
    i += 1
    
    if (i >= len(sentence_list)):
        i = -1

    return str(i);
