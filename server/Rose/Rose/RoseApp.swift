//
//  RoseApp.swift
//  Rose
//
//  Created by Clinton Page on 13/3/2024.
//

import SwiftUI

@main
struct RoseApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) private var appDelegate
    let listener = HttpListener()
    
    init() {
        //Start the web server
        listener.startListening(port: 8081)
    }
    
    var body: some Scene {
        Settings {
            EmptyView()
        }
    }
}

class AppDelegate : NSObject, NSApplicationDelegate {
    static private(set) var instance: AppDelegate!
    
    lazy var statusBarItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
    
    let menu = ApplicationMenu()
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        AppDelegate.instance = self
        
        statusBarItem.button?.image = NSImage(named: NSImage.Name("rose"))
        statusBarItem.button?.imagePosition = .imageLeading
        statusBarItem.menu = menu.createMenu()
    }
}
