//
//  RdpManager.swift
//  Rose
//
//  Created by Clinton Page on 15/3/2024.
//

import Foundation
import AppKit


class RdpManager {
    func rdpToRemoteApp(ipAddress: String, appRequest : RdpRequest) {
        let rdpFile = self.generateRdpFile(ipAddress: ipAddress, appRequest: appRequest)
        
        //Open with RDP client
        NSWorkspace.shared.open(rdpFile)
    }
    
    func generateRdpFile(ipAddress: String, appRequest: RdpRequest) -> URL {
        let directory = NSTemporaryDirectory()
        let fileName = "rose-connect.rdp"

        let tempUrl : URL = NSURL.fileURL(withPathComponents: [directory, fileName])!
        let rdpContent = """
            remoteapplicationname:s:\(appRequest.DisplayName)
            full address:s:\(ipAddress)
            prompt for credentials on client:i:0
            authentication level:i:3
            promptcredentialonce:i:0
            autoreconnection enabled:i:1

            redirected video capture encoding quality:i:2
            disable menu anims:i:0
            disable cursor setting:i:1
            allow font smoothing:i:1
            audiocapturemode:i:1

            Remoteapplicationmode:i:1
            remoteapplicationprogram:s:"\(appRequest.Command)"
            remoteapplicationcmdline:s:
            """
        try? rdpContent.write(to: tempUrl, atomically: true, encoding: .utf8)
        return tempUrl
    }
}
