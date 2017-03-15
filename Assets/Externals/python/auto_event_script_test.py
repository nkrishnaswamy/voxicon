import sys
import os
#import sentence_list

def send_next_event_to_port(list_file, index, port):
    sys.path.append('../../../../../../Dissertation/MAE/SentenceSimulability/test-sets/')
    #    sys.path.append('/Users/nikhil/Documents/Homework/Brandeis/Dissertation/MAE/SentenceSimulability/test-sets/')
    module_obj = __import__(list_file, fromlist = [''])
    sentences = module_obj.sentences
    
    i = int(index)
    os.system('''echo "%s" | nc -w 0 localhost %s''' % (sentences[i], port))
    i += 1
    
    if (i >= len(sentences)):
        i = -1

    return str(i)

def main():
    send_next_event_to_port("open_close", "0", "7777")

if __name__ == "__main__":
    main()
