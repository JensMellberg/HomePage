class Alert {
	static openAlert(message) {
		const okButton = HtmlUtils.createElement('button', 'sectionButton');
		okButton.innerText = 'Ok';
		$(okButton).click(() => Popup.closePopup());
		Alert.createAlertBox(message, null, [okButton]);
	}

	static openDialog(message, buttons) {
		Alert.openDialogInternal(message, null, buttons)
	}

	static openDialogWithElement(element, buttons) {
		Alert.openDialogInternal(null, element, buttons)
	}

	static openDialogInternal(message, element, buttons) {
		const buttonElements = [];
		for (const buttonData of buttons) {
			const button = HtmlUtils.createElement('button', 'sectionButton');
			button.innerText = buttonData.text;
			$(button).click(() => buttonData.action());
			buttonElements.push(button)
		}

		Alert.createAlertBox(message, element, buttonElements);
	}

	static createAlertBox(message, element, buttons) {
		const alertBox = HtmlUtils.createElement('div', 'alertBox');
		if (message) {
			const heading = HtmlUtils.addElement('span', 'alertMessage', alertBox);
			heading.innerText = message;
		} else if (element) {
			alertBox.appendChild(element);
		}
		
		const buttonWrapper = HtmlUtils.addElement('div', 'alertButtons', alertBox);
		for (const btn of buttons) {
			buttonWrapper.appendChild(btn);
		}

		Popup.putOnOverLay(alertBox, false);
	}
}