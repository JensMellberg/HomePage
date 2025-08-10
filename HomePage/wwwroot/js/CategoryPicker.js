class CategoryPicker {
	static createPicker(categories, callback) {
		const wrapper = HtmlUtils.createElement('div', 'category-picker');
		const wrapperInner = HtmlUtils.addElement('div', 'category-picker-inner', wrapper);
		let allPicked = [];
		for (const category of categories) {
			const localId = category.id;
			const localName = category.name;
			const elem = HtmlUtils.addElement('div', 'category', wrapperInner);
			if (category.isSelected) {
				elem.classList.add('selected');
				allPicked.push({ id: localId, name: localName });
			}

			elem.innerText = category.name;
			$(elem).click(() => {
				if (allPicked.map(x => x.id).includes(localId)) {
					allPicked = allPicked.filter(x => x.id != localId);
					elem.classList.remove('selected')
				}
				else {
					allPicked.push({ id: localId, name: localName });
					elem.classList.add('selected')
				}
			})
		}

		const okButton = HtmlUtils.addElement('button', 'sectionButton', wrapper);
		okButton.type = 'button'
		okButton.innerText = 'Klar';
		$(okButton).click(() => {
			Popup.closePopup()
			callback(allPicked)
		});

		Popup.putOnOverLay(wrapper, false);
	}
}