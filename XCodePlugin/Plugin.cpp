/*
	This is a simple plugin, a bunch of functions that do simple things.
*/

#include <CoreFoundation/CoreFoundation.h>
#include <sys/socket.h>
#include <netinet/in.h>

#include "Plugin.pch"


bool OpenPort(int port) {
    int ipv4_socket = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
    //int ipv6_socket = socket(PF_INET6, SOCK_STREAM, IPPROTO_TCP);
    bool r = false;
    
    struct sockaddr_in sin;
    memset(&sin, 0, sizeof(sin));
    sin.sin_len = sizeof(sin);
    sin.sin_family = AF_INET; // or AF_INET6 (address family)
    sin.sin_port = htons(port);
    sin.sin_addr.s_addr = INADDR_ANY;
    
    if (bind(ipv4_socket, (struct sockaddr *)&sin, sizeof(sin)) < 0) {
        // Handle the error
    }
    else {
        if (listen(ipv4_socket,1) < 0) {
            // Handle the error
        }
        else {
            r = true;
        }
    }
    
    return r;
}



