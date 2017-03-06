import os
import sentence_list

def send_next_event_to_port(index, port):
    sentences = sentence_list.sentences
                     
    i = int(index)
    os.system('''echo "%s" | nc -w 0 localhost %s''' % (sentences[i], port))
    i += 1
    
    if (i >= len(sentences)):
        i = -1

    return str(i);
