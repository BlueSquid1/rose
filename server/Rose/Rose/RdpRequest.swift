//
//  rdpRequest.swift
//  Rose
//
//  Created by Clinton Page on 13/3/2024.
//

import Foundation

struct RdpRequest : Decodable {
    let DisplayName: String
    let Command: String
    let Arguements: String
}
