# Chat-Net

## Description

TODO

## High Level Requirements

TODO

## Repository Structure

* */doc* - Contains project documentation.
* */src* - Contains project source code.

## Source Code Structure

* */src/Client* - Source code of client application.

* */src/ClientTests* - Source code of unit tests for client application.

* */src/Server* - Source code of server application.

* */src/ServerTests* - Source code of unit tests for server application.

* */src/CommonUtilities* - Source code of library containing functionalities used both by server and client application.

* */src/CommonUtilities* - Source code of unit tests for common utilities library.

## Simple Session Layer Protocol

### Overview

When dealing with TCP sockets it is not guaranteed, that exactly one call of Socket.Sent on the sender site will result in exactly one call of Socket.Receive on the recipient site - there is a need to check if particular patch of data was received fully or partially.

Above mentioned need was the origin of Simple Session Layer Protocol (SSLP) - trivial and easy to implement protocol operating on [session (5th) layer](https://en.wikipedia.org/wiki/Session_layer) of [OSI model](https://en.wikipedia.org/wiki/OSI_model), created for sake of this project.

### Packet structure

Patch of data (packet payload) sent through the socket is preceded by fixed length header. Sum of bytes creating a header indicates the length of the payload.

**Example 1:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0] (decimal notation). Length of received data is smaller than header length, which means that only part of packet was received. Recipient needs to wait for remaining part of data.

**Example 2:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0, 5, 1, 2] (decimal notation). Sequence can be spited into header part, which are first 3 bytes [0, 0, 5] and payload part [1, 2], which proceeds the header. Sum of bytes creating a header is equal to 5, but payload consists of only 2 bytes, which means that recipient received partial packet and needs to wait for missing 3 bytes.

**Example 2:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0 0 3 1 2 3] (decimal notation). Sequence can be spited into header part, which are first 3 bytes [0 0 3] and payload part [1 2 3], which proceeds the header. Sum of bytes creating a header is equal to 3 and payload consists of only 3 bytes, which means that packet was fully received. Recipient can process int further.

## Used Tools

* IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
* Documentation generator: [DoxyGen 1.12.0](https://www.doxygen.nl/)

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka)
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/)

## License

This project is licensed under the MIT License - see the *LICENSE.md* file for details.
