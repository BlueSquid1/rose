//
//  RoseApp.swift
//  Rose
//
//  Created by Clinton Page on 13/3/2024.
//

import SwiftUI

import Swifter
import Dispatch

@main
struct RoseApp: App {
    
    @NSApplicationDelegateAdaptor(AppDelegate.self) private var appDelegate
    
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}

class AppDelegate : NSObject, NSApplicationDelegate, ObservableObject {
    private var statusItem: NSStatusItem!
    private var popover: NSPopover!
    
    func startWebServer() {
        let server = HttpServer()
        server["/hello"] = { .ok(.htmlBody("You asked for \($0)"))  }
        do{
            try server.start(9080, forceIPv4: true)
            print("Server has started ( port = \(try server.port()) ). Try to connect now...")
        } catch {
            print("Server start error: \(error)")
        }
    }
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        //Start the web server
        self.startWebServer()
        
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        
        if let statusButton = statusItem.button {
            statusButton.image = NSImage(systemSymbolName: "chart.line.uptrend.xyaxis.circle", accessibilityDescription: "RDP server listener")
            statusButton.action = #selector(togglePopup)
        }
        
        self.popover = NSPopover()
        self.popover.contentSize = NSSize(width: 300, height: 300)
        // Will disapper if you click somewhere else
        self.popover.behavior = .transient
        self.popover.contentViewController = NSHostingController(rootView: ContentView())
    }
    
    @objc func togglePopup() {
        if let button = statusItem.button {
            if popover.isShown {
                self.popover.performClose(nil)
            } else {
                popover.show(relativeTo: button.bounds, of: button, preferredEdge: NSRectEdge.minY)
            }
        }
    }
}
