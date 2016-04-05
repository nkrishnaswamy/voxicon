//
//  ViewController.h
//  VoxSim Commander
//
//  Created by Nikhil Krishnaswamy on 3/24/16.
//  Copyright © 2016 Nikhil Krishnaswamy. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface ViewController : UIViewController<NSStreamDelegate>
{
    CFReadStreamRef readStream;
    CFWriteStreamRef writeStream;
    
    NSInputStream   *inputStream;
    NSOutputStream  *outputStream;
    
    NSMutableArray  *messages;
}

@property (weak, nonatomic) IBOutlet UITextField *ipAddressText;
@property (weak, nonatomic) IBOutlet UITextField *portText;
@property (weak, nonatomic) IBOutlet UITextField *dataToSendText;
@property (weak, nonatomic) IBOutlet UITextView *dataRecievedTextView;
@property (weak, nonatomic) IBOutlet UILabel *connectedLabel;
@property (weak, nonatomic) IBOutlet UIButton *connectButton;
@property (weak, nonatomic) IBOutlet UIButton *disconnectButton;
@property (weak, nonatomic) IBOutlet UIButton *sendButton;
@property (weak, nonatomic) IBOutlet UIScrollView *presetsView;

@end

