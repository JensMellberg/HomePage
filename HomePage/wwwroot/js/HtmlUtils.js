class HtmlUtils {
	static createElement(tag, classList) {
		var element = document.createElement(tag);
		if (classList) {
			element.classList = classList;
		}

		return element;
	}

	static addElement(tag, classList, parent) {
		var element = HtmlUtils.createElement(tag, classList);
		parent.appendChild(element);

		return element;
	}

	static fillDropdown(element, options, selectedId) {
		for (const option of options) {
			if (!option.id) {
				return
			}

			const optionElement = HtmlUtils.addElement('option', '', element)
			optionElement.value = option.id
			optionElement.innerText = option.text
			optionElement.selected = option.id === selectedId
		}
	}
}
