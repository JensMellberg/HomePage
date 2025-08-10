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
}
