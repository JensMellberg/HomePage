class JsonResultUnpacker {
	static unpackResult(result, window) {
		if (result.redirectUrl) {
			window.location.href = result.redirectUrl;
			return null;
		}

		if (result.success === false) {
			if (result.message) {
				Alert.openAlert(result.message)
			}
			
			return null;
		}

		return result.data;
	}
}
