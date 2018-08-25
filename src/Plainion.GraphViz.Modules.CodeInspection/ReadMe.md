 

- arch
  - actor
  - client
  - view
  - viewmodel
  - analyzer contains all logic

- create custom request/response messages
  - respone needs to derive from finish
  - naming convention? "GetAllTypesRequest/Response"?
- derive from base clases

- guideline where to do threading
  - just in analyzer today
  - when we really model threading with akka then still logic in analyzer

- use document serializer