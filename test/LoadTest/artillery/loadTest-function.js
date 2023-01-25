module.exports = {
    setVariables: setVariables,
    logHeaders: logHeaders,
    logBody: logBody
  }
  
  function setVariables(requestParams, context, ee, next) {
    
    context.vars.userId = generate_guid();
    context.vars.correlationId = generate_guid();
    return next(); // MUST be called for the scenario to continue
  }
  
  function logHeaders(requestParams, response, context, ee, next) {
    console.log("---------------------------------------------------------------");
    console.log("---------------------------------------------------------------");    
    console.log("---------------------------------------------------------------");
    console.log(JSON.stringify( response.headers));
    return next(); // MUST be called for the scenario to continue
  }
  
  function logBody(requestParams, response, context, ee, next) {
    console.log("---------------------------------------------------------------");
    console.log("---------------------------------------------------------------");    
    console.log("---------------------------------------------------------------");
    console.log(JSON.stringify( response.body));
    return next(); // MUST be called for the scenario to continue
  }
  function generate_guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);
        return v.toString(16);
    });
}