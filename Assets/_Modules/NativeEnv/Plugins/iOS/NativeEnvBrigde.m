#import <Foundation/Foundation.h>

extern "C" {

    void iOSSet(const char* key, const char* value) {
        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSString *nsValue = value ? [NSString stringWithUTF8String:value] : @"";
        [[NSUserDefaults standardUserDefaults] setObject:nsValue forKey:nsKey];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }

    const char* iOSGet(const char* key, const char* defaultValue) {
        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSString *val = [[NSUserDefaults standardUserDefaults] stringForKey:nsKey];
        if (!val) val = [NSString stringWithUTF8String:defaultValue ?: ""];
        return strdup([val UTF8String]);
    }
}