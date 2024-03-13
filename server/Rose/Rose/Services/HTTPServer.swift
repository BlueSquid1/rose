//
//  WebServer.swift
//  Rose
//
//  Created by Clinton Page on 13/3/2024.
//

import Foundation
import Network

class HTTPServer {
    static let shared = HTTPServer()
    var listener : NWListener
    var queue : DispatchQueue
    var connected : Bool = false
    let nwParms = NWParameters.tcp
    
    init() {
        queue = DispatchQueue(label: "HTTP Server Queue")
        listener = try! NWListener(using: nwParms, on: 80)
        listener.newConnectionHandler = { [weak self] (newConnection ) in
            print("**** New Connection added")
            if let strongSelf = self {
                newConnection.start(queue: strongSelf.queue)
                strongSelf.receive(on: newConnection)
            }
        }

        listener.stateUpdateHandler = { (newState) in
            print("**** Listener changed state to \(newState)")
        }

        listener.start(queue: queue)
    }

    func receive(on connection: NWConnection) {
        connection.receive(minimumIncompleteLength: 0, maximumLength: 4096) { (content, context, isComplete, error) in
            guard let receivedData = content else {
                print("**** content is nil")
                return
            }
            let dataString = String(decoding: receivedData, as: UTF8.self)
            print("**** received data = \(dataString)")
            connection.cancel()
        }
    }
}
