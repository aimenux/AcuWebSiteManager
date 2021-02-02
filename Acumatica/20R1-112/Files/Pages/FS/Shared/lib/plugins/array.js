// Array compare

Array.prototype.compare = function(array) {
    // if the other array is a falsy value, return
    if (!array)
        return false;

    // compare lengths - can save a lot of time
    if (this.length != array.length)
        return false;

    for (var i = 0; i < this.length; i++) {
        // Check if we have nested arrays
        if (this[i] instanceof Array && array[i] instanceof Array) {
            // recurse into the nested arrays
            if (!this[i].compare(array[i]))
                return false;
        } else if (this[i] != array[i]) {
            // Warning - two different object instances will never be equal: {x:20} != {x:20}
            return false;
        }
    }
    return true;
}

// clone an array
Array.prototype.clone = function() {
    var result = [];
    for (var key in this) {
        result[key] = this[key];
    }
    return result;
}

//has Value
Array.prototype.hasValue = function(value) {
    var i;
    for (i = 0; i < this.length; i++) {
        if (this[i] === value) return true;
    }
    return false;
}