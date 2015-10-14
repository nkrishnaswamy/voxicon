/*
	This is a simple plugin, a bunch of functions that do simple things.
*/

#include <CoreFoundation/CoreFoundation.h>

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>

#include "Plugin.pch"

#define BUFSIZE 2048

int ipv4_socket, new_fd;
int recvlen;
struct sockaddr_in remaddr;
socklen_t addrlen;
unsigned char buf[BUFSIZE];

fd_set master;    // master file descriptor list
fd_set read_fds;  // temp file descriptor list for select()
int fdmax;        // maximum file descriptor number

int listener;     // listening socket descriptor
int newfd;        // newly accept()ed socket descriptor
struct sockaddr_storage remoteaddr; // client address

int nbytes;

char remoteIP[INET6_ADDRSTRLEN];
int i, j, rv;

struct addrinfo hints, *ai, *p;

// get sockaddr, IPv4 or IPv6:
void *get_in_addr(struct sockaddr *sa)
{
    if (sa->sa_family == AF_INET) {
        return &(((struct sockaddr_in*)sa)->sin_addr);
    }
    
    return &(((struct sockaddr_in6*)sa)->sin6_addr);
}

bool OpenPort(char *port) {
    int yes = 1;        // for setsockopt() SO_REUSEADDR, below
    
    bool r = false;
    
    FD_ZERO(&master);    // clear the master and temp sets
    FD_ZERO(&read_fds);
    
    // get us a socket and bind it
    memset(&hints, 0, sizeof hints);
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_flags = AI_PASSIVE;
    if ((rv = getaddrinfo(NULL, port, &hints, &ai)) != 0) {
        // Handle the error
    }
    else {
        for(p = ai; p != NULL; p = p->ai_next) {
            listener = socket(p->ai_family, p->ai_socktype, p->ai_protocol);
            if (listener < 0) {
                continue;
            }
            
            // lose the pesky "address already in use" error message
            setsockopt(listener, SOL_SOCKET, SO_REUSEADDR, &yes, sizeof(int));
            
            // put socket in nonblocking mode
            fcntl(listener, F_GETFL, O_NONBLOCK);

            if (bind(listener, p->ai_addr, p->ai_addrlen) < 0) {
                // Handle the error
            }
            
            break;
        }
        
        // if we got here, it means we didn't get bound
        if (p == NULL) {
            // Handle the error
        }
        else {
            freeaddrinfo(ai); // all done with this
            
            // listen
            if (listen(listener, 10) == -1) {
                // Handle the error
            }
            else {
                // add the listener to the master set
                FD_SET(listener, &master);
                
                // keep track of the biggest file descriptor
                fdmax = listener; // so far, it's this one
                
                r = true;
            }
        }
    }
    
    return r;
}

unsigned char *Process() {
    memset(buf,0,sizeof(buf));
    
    read_fds = master; // copy it
    if (select(fdmax+1, &read_fds, NULL, NULL, NULL) == -1) {
        // Handle the error
    }
    
    // run through the existing connections looking for data to read
    for(i = 0; i <= fdmax; i++) {
        if (FD_ISSET(i, &read_fds)) { // we got one!!
            if (i == listener) {
                // handle new connections
                addrlen = sizeof(remoteaddr);
                newfd = accept(listener,
                               (struct sockaddr *)&remoteaddr,
                               &addrlen);
                
                if (newfd == -1) {
                    //perror("accept");
                } else {
                    FD_SET(newfd, &master); // add to master set
                    if (newfd > fdmax) {    // keep track of the max
                        fdmax = newfd;
                    }
  
                    inet_ntop(remoteaddr.ss_family,
                              get_in_addr((struct sockaddr*)&remoteaddr),
                              remoteIP, INET6_ADDRSTRLEN);
                }
            } else {
                // handle data from a client
                if ((nbytes = recv(i, buf, sizeof(buf), 0)) <= 0) {
                    // got error or connection closed by client
                    if (nbytes == 0) {
                        // Handle the error
                    } else {
                        // Handle the error
                    }
                } else {
                    // we got some data from a client
                }
            }
        }
    }
    
    return buf;
}

bool ClosePort(char *port) {
    bool r = false;
    
    //if ((rv = getaddrinfo(NULL, port, &hints, &ai)) != 0) {
        // Handle the error
    //}
    //else {
        close(listener);
        r = true;
    //}
    
    return r;
}



