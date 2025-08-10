class Popup {
	static openPopup(content, closeOnClick) {
		const contentWrapper = document.createElement('div');
		$(contentWrapper).click(() => false);
		contentWrapper.classList.add('popupContent');
		contentWrapper.appendChild(content);
		Popup.putOnOverLay(contentWrapper, closeOnClick);
	}

	static putOnOverLay(content, closeOnClick, callback) {
		const overlay = document.createElement('div');
		overlay.classList.add('popupOverlay');

		if (callback) {
			$(overlay).click(() => callback());
		} else if (closeOnClick) {
			$(overlay).click(() => Popup.closePopup());
		}

		overlay.appendChild(content);
		$('main').append(overlay);
	}

	static closePopup() {
		$('.popupOverlay').remove();
	}
}
