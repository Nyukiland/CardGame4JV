#import <UIKit/UIKit.h>

extern "C" {
    void _CopyToClipboard(const char* text)
    {
        NSString* nsText = [NSString stringWithUTF8String:text];
        if (nsText != nil) {
            [UIPasteboard generalPasteboard].string = nsText;
        }
    }
}
