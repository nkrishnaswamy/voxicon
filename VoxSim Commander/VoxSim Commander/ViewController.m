//
//  ViewController.m
//  VoxSim Commander
//
//  Created by Nikhil Krishnaswamy on 3/24/16.
//  Copyright © 2016 Nikhil Krishnaswamy. All rights reserved.
//

#import "ViewController.h"

@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    _connectedLabel.text = @"Disconnected";
    [_connectedLabel setTextAlignment:NSTextAlignmentCenter];
    
    UITapGestureRecognizer *tap = [[UITapGestureRecognizer alloc]
                                   initWithTarget:self
                                   action:@selector(dismissKeyboard)];
    
    [self.view addGestureRecognizer:tap];
    
    [_connectButton setBackgroundColor:[UIColor colorWithRed:0.80 green:0.80 blue:0.90 alpha:1.0]];
    [[_connectButton layer] setBorderWidth:1.0f];
    [[_connectButton layer] setBorderColor:[UIColor colorWithRed:0.80 green:0.80 blue:0.80 alpha:1.0].CGColor];
    [[_connectButton layer] setCornerRadius:5.0f];
    //[_connectButton setBackgroundImage:[UIImage imageNamed:@"glossybutton.png"] forState:UIControlStateNormal];
    [_connectButton addTarget:self action:@selector(buttonClicked:) forControlEvents:UIControlEventTouchDown];
    [_connectButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchUpInside];
    [_connectButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchUpOutside];
    [_connectButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchDragOutside];
    [_connectButton setEnabled:false];
    
    [_disconnectButton setBackgroundColor:[UIColor colorWithRed:0.80 green:0.80 blue:0.90 alpha:1.0]];
    [[_disconnectButton layer] setBorderWidth:1.0f];
    [[_disconnectButton layer] setBorderColor:[UIColor colorWithRed:0.80 green:0.80 blue:0.80 alpha:1.0].CGColor];
    [[_disconnectButton layer] setCornerRadius:5.0f];
    //[_disconnectButton setBackgroundImage:[UIImage imageNamed:@"glossybutton.png"] forState:UIControlStateNormal];
    [_disconnectButton addTarget:self action:@selector(buttonClicked:) forControlEvents:UIControlEventTouchDown];
    [_disconnectButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchUpInside];
    [_disconnectButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchUpOutside];
    [_disconnectButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchDragOutside];
    [_disconnectButton setEnabled:false];
    
    [_sendButton setBackgroundColor:[UIColor colorWithRed:0.80 green:0.80 blue:0.90 alpha:1.0]];
    [[_sendButton layer] setBorderWidth:1.0f];
    [[_sendButton layer] setBorderColor:[UIColor colorWithRed:0.80 green:0.80 blue:0.80 alpha:1.0].CGColor];
    [[_sendButton layer] setCornerRadius:5.0f];
    //[_sendButton setBackgroundImage:[UIImage imageNamed:@"glossybutton.png"] forState:UIControlStateNormal];
    [_sendButton addTarget:self action:@selector(buttonClicked:) forControlEvents:UIControlEventTouchDown];
    [_sendButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchUpInside];
    [_sendButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchUpOutside];
    [_sendButton addTarget:self action:@selector(buttonReleased:) forControlEvents:UIControlEventTouchDragOutside];
    [_sendButton setEnabled:false];
    
    _ipAddressText.clearButtonMode = UITextFieldViewModeWhileEditing;
    _portText.clearButtonMode = UITextFieldViewModeWhileEditing;
    _dataToSendText.clearButtonMode = UITextFieldViewModeWhileEditing;
    
    [_ipAddressText addTarget:self action:@selector(fieldInputChanged:) forControlEvents:UIControlEventEditingChanged];
    [_portText addTarget:self action:@selector(fieldInputChanged:) forControlEvents:UIControlEventEditingChanged];
    [_dataToSendText addTarget:self action:@selector(dataInputChanged:) forControlEvents:UIControlEventEditingChanged];
    
    // read prefs
    NSString *path = [[NSBundle mainBundle] pathForResource:@"prefs" ofType:@"txt"];
    NSString *content = [NSString stringWithContentsOfFile:path encoding:NSUTF8StringEncoding error:NULL];
    NSArray *arr = [content componentsSeparatedByString:@":"];
    
    _ipAddressText.text = arr[0];
    _portText.text = arr[1];
    
    [_presetsView setBackgroundColor:[UIColor colorWithRed:0.90 green:0.90 blue:0.95 alpha:1.0]];
    [[_presetsView layer] setBorderWidth:2.0f];
    [[_presetsView layer] setBorderColor:[UIColor colorWithRed:0.75 green:0.75 blue:0.75 alpha:1.0].CGColor];
    [[_presetsView layer] setCornerRadius:5.0f];
    
    // read any presets
    path = [[NSBundle mainBundle] pathForResource:@"presets" ofType:@"txt"];
    content = [NSString stringWithContentsOfFile:path encoding:NSUTF8StringEncoding error:NULL];
    arr = [content componentsSeparatedByString:@"\n"];

    if ([arr count] > 0) {
        for (int i = 0; i < [arr count]; i++) {
            UIButton *button = [[UIButton alloc] initWithFrame:CGRectMake(6,(i*45)+6,_presetsView.frame.size.width-12,40)];
            [button setTitle:[NSString stringWithFormat:@"%@", arr[i]] forState:UIControlStateNormal];
            [button setTag:i];
            [button setBackgroundColor:[UIColor colorWithRed:0.10 green:0.10 blue:0.20 alpha:1.0]];
            [button setTitleColor:[UIColor grayColor] forState:UIControlStateDisabled];
            [button setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
            [[button layer] setBorderWidth:1.0f];
            [[button layer] setBorderColor:[UIColor colorWithRed:0.25 green:0.25 blue:0.25 alpha:1.0].CGColor];
            [[button layer] setCornerRadius:5.0f];
            [button addTarget:self action:@selector(cmdButtonClicked:) forControlEvents:UIControlEventTouchDown];
            [button addTarget:self action:@selector(sendMessage:) forControlEvents:UIControlEventTouchUpInside];
            [button addTarget:self action:@selector(cmdButtonReleased:) forControlEvents:UIControlEventTouchUpInside];
            [button addTarget:self action:@selector(cmdButtonReleased:) forControlEvents:UIControlEventTouchUpOutside];
            [button addTarget:self action:@selector(cmdButtonReleased:) forControlEvents:UIControlEventTouchCancel];
            [button setEnabled:false];
            
            [_presetsView addSubview:button];
        }
    }
}

- (void) viewDidLayoutSubviews {
    [super viewDidLayoutSubviews];
    
    _presetsView.contentSize = CGSizeMake(_presetsView.frame.size.width, (_presetsView.subviews.count*45)+46);
}

- (IBAction) sendMessage {
    NSString *response  = [NSString stringWithFormat:@"%@", _dataToSendText.text];
    NSData *data = [[NSData alloc] initWithData:[response dataUsingEncoding:NSASCIIStringEncoding]];
    [outputStream write:[data bytes] maxLength:[data length]];
}

- (void) sendMessage:(id )sender {
    UIButton *button = (UIButton*)sender;
    _dataToSendText.text = button.currentTitle;
    
    //NSString *response  = [NSString stringWithFormat:@"%@", _dataToSendText.text];
    NSData *data = [[NSData alloc] initWithData:[button.currentTitle dataUsingEncoding:NSASCIIStringEncoding]];
    [outputStream write:[data bytes] maxLength:[data length]];
}

- (void) messageReceived:(NSString *)message {
    [messages addObject:message];
    
    _dataRecievedTextView.text = message;
    NSLog(@"%@", message);
}

- (void)stream:(NSStream *)theStream handleEvent:(NSStreamEvent)streamEvent {
    
    NSLog(@"stream event %lu", streamEvent);
    
    switch (streamEvent) {
            
        case NSStreamEventOpenCompleted:
            NSLog(@"Stream opened");
            _connectedLabel.text = [NSString stringWithFormat:@"Connected to %@:%@", _ipAddressText.text, _portText.text];
            [_disconnectButton setEnabled:true];
            
            if (_dataToSendText.text.length > 0) {
                [_sendButton setEnabled:true];
            }
            
            for (int i = 0; i < [[_presetsView subviews] count]; i++) {
                UIButton *button = (UIButton *)[_presetsView subviews][i];
                [button setEnabled:true];
            }
            break;
        case NSStreamEventHasBytesAvailable:
            
            if (theStream == inputStream) {
                uint8_t buffer[1024];
                NSInteger len;
                
                while ([inputStream hasBytesAvailable]) {
                    len = [inputStream read:buffer maxLength:sizeof(buffer)];
                    if (len > 0) {
                        NSString *output = [[NSString alloc] initWithBytes:buffer length:len encoding:NSASCIIStringEncoding];
                        
                        if (nil != output) {
                            NSLog(@"server said: %@", output);
                            [self messageReceived:output];
                        }
                    }
                }
            }
            break;
            
        case NSStreamEventHasSpaceAvailable:
            NSLog(@"Stream has space available now");
            break;
            
        case NSStreamEventErrorOccurred:
            NSLog(@"%@",[theStream streamError].localizedDescription);
            _connectedLabel.text = @"Unable to connect";
            break;
            
        case NSStreamEventEndEncountered:
            [theStream close];
            [theStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
            _connectedLabel.text = @"Disconnected";
            NSLog(@"close stream");
            break;
            
        default:
            NSLog(@"Unknown event");
    }
    
}

- (IBAction)connectToServer:(id)sender {
    
    NSLog(@"Setting up connection to %@ : %i", _ipAddressText.text, [_portText.text intValue]);
    CFStreamCreatePairWithSocketToHost(kCFAllocatorDefault, (__bridge CFStringRef) _ipAddressText.text, [_portText.text intValue], &readStream, &writeStream);
    
    messages = [[NSMutableArray alloc] init];
    
    [self open];
}

- (IBAction)disconnect:(id)sender {
    
    [self close];
}

- (void)open {
    
    NSLog(@"Opening streams.");
    
    outputStream = (__bridge NSOutputStream *)writeStream;
    inputStream = (__bridge NSInputStream *)readStream;
    
    [outputStream setDelegate:self];
    [inputStream setDelegate:self];
    
    [outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    
    [outputStream open];
    [inputStream open];
    
    _connectedLabel.text = @"Attempting to connect... ";
}

- (void)close {
    NSLog(@"Closing streams.");
    [inputStream close];
    [outputStream close];
    [inputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [outputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [inputStream setDelegate:nil];
    [outputStream setDelegate:nil];
    inputStream = nil;
    outputStream = nil;
    
    NSString *path = [[NSBundle mainBundle] pathForResource:@"prefs" ofType:@"txt"];
    NSString *str = [NSString stringWithFormat:@"%@:%@", _ipAddressText.text, _portText.text];
    NSData *data = [str dataUsingEncoding:NSUTF8StringEncoding];
    [data writeToFile:path atomically:NO];
    
    _connectedLabel.text = @"Disconnected";
    [_sendButton setEnabled:false];
    [_disconnectButton setEnabled:false];
    for (int i = 0; i < [[_presetsView subviews] count]; i++) {
        UIButton *button = (UIButton *)[_presetsView subviews][i];
        [button setEnabled:false];
    }
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (IBAction) buttonClicked:(id)sender {
    UIButton *button = (UIButton *)sender;
    button.backgroundColor = [UIColor colorWithRed:0.70 green:0.70 blue:0.80 alpha:1.0];
}

- (IBAction) cmdButtonClicked:(id)sender {
    UIButton *button = (UIButton *)sender;
    button.backgroundColor = [UIColor colorWithRed:0.20 green:0.20 blue:0.30 alpha:1.0];
}

- (IBAction) buttonReleased:(id)sender {
    UIButton *button = (UIButton *)sender;
    button.backgroundColor = [UIColor colorWithRed:0.80 green:0.80 blue:0.90 alpha:1.0];
}

- (IBAction) cmdButtonReleased:(id)sender {
    UIButton *button = (UIButton *)sender;
    button.backgroundColor = [UIColor colorWithRed:0.10 green:0.10 blue:0.20 alpha:1.0];
}

- (IBAction) fieldInputChanged:(id)sender {
    if ((_ipAddressText.text.length > 0) && (_portText.text.length > 0)){
        [_connectButton setEnabled:true];
    }
    else {
        [_connectButton setEnabled:false];
    }
}

- (IBAction) dataInputChanged:(id)sender {
    if (_dataToSendText.text.length > 0){
        [_sendButton setEnabled:true];
    }
    else {
        [_sendButton setEnabled:false];
    }
}

-(void)dismissKeyboard {
    [_ipAddressText resignFirstResponder];
    [_portText resignFirstResponder];
    [_dataToSendText resignFirstResponder];
}

@end
