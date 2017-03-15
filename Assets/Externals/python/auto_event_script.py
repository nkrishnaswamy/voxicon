import sys
import os

def send_next_event_to_port(dir, list_file, index, port):
    sys.path.append(dir)
    module_obj = __import__(list_file, fromlist = [''])
    sentences = module_obj.sentences
    
    i = int(index)
    os.system('''echo "%s" | nc -w 0 localhost %s''' % (sentences[i], port))
    i += 1
    
    if (i >= len(sentences)):
        i = -1

    return str(i)
