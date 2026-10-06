var LibraryWebSockets = {
$webSocketInstances: [],
$webSocketDynCall: function(signature, callback)
{
    var args = Array.prototype.slice.call(arguments, 2);
    var dynCall = Module['dynCall_' + signature];
    if (dynCall)
    {
        return dynCall.apply(null, [callback].concat(args));
    }

    var table = typeof wasmTable !== 'undefined' ? wasmTable : Module['wasmTable'];
    var wasmFunction = table && table.get(callback);
    if (wasmFunction)
    {
        return wasmFunction.apply(null, args);
    }

    if (typeof getWasmTableEntry === 'function')
    {
        return getWasmTableEntry(callback).apply(null, args);
    }

    throw new Error('Unable to invoke WebSocket callback with signature ' + signature);
},

SocketCreate: function(url, protocols, openCallback, recvCallback, errorCallback, closeCallback)
{
    var str = UTF8ToString(url);
    var prot = UTF8ToString(protocols);
    var socket = {
        socket: new WebSocket(str, [prot]),
        error: null,
    }
    var instance = webSocketInstances.push(socket) - 1;
    socket.socket.binaryType = 'arraybuffer';
    
    socket.socket.onopen = function () {
        webSocketDynCall('vi', openCallback, instance);
    }
    socket.socket.onmessage = function (e) {
        if (e.data instanceof ArrayBuffer)
        {
            const b = e.data;
            const ptr = _malloc(b.byteLength);
            const dataHeap = new Int8Array(HEAPU8.buffer, ptr, b.byteLength);
            dataHeap.set(new Int8Array(b));
            webSocketDynCall('viii', recvCallback, instance, ptr, b.byteLength);
            _free(ptr);
        }
    };
    socket.socket.onerror = function (e) {
        webSocketDynCall('vii', errorCallback, instance, e.code || 0);
    }
    socket.socket.onclose = function (e) {
        if (e.code != 1000)
        {
            webSocketDynCall('vii', closeCallback, instance, e.code || 0);
        }
    }
    return instance;
},

SocketState: function (socketInstance)
{
    var socket = webSocketInstances[socketInstance];
    return socket.socket.readyState;
},

SocketError: function (socketInstance, ptr, bufsize)
{
 	var socket = webSocketInstances[socketInstance];
 	if (socket.error == null)
 		return 0;
    stringToUTF8(socket.error, ptr, bufsize);
    return 1;
},

SocketSend: function (socketInstance, ptr, length)
{
    var socket = webSocketInstances[socketInstance];
    socket.socket.send (HEAPU8.buffer.slice(ptr, ptr+length));
},

SocketClose: function (socketInstance)
{
    var socket = webSocketInstances[socketInstance];
    socket.socket.close();
}
};

autoAddDeps(LibraryWebSockets, '$webSocketInstances');
autoAddDeps(LibraryWebSockets, '$webSocketDynCall');
mergeInto(LibraryManager.library, LibraryWebSockets);
