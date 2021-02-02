// check if the supplied string is empty or null
Function.prototype.isEmptyOrNull = function(string) {
	if (string == null || string == "") {
		return true
	}
	return false;
};