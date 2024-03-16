//
//  WebServer.swift
//  Rose
//
//  Created by Clinton Page on 13/3/2024.
//

import Foundation
import Swifter
import Dispatch
import Darwin

class HttpListener {
    
    let server : HttpServer
    let rdpManager : RdpManager
    
    init() {
        self.server = HttpServer()
        self.rdpManager = RdpManager()
        
        self.server.POST["/request"] = { request in
            let message = Data(request.body)
            let decoder = JSONDecoder()
            do {
                let rdpRequest = try decoder.decode(RdpRequest.self, from: message)
                
                self.rdpManager.rdpToRemoteApp(ipAddress: request.address!, appRequest: rdpRequest)
                
            } catch {
                print(error.localizedDescription)
                return .internalServerError
            }
            return .ok(.htmlBody("ok !"))
        }
    }
    func startListening(port: in_port_t) {
        do{
            try self.server.start(port, forceIPv4: true)
            print("Server has started ( port = \(try self.server.port()) ). Try to connect now...")
        } catch {
            print("Server start error: \(error)")
        }
    }
}
