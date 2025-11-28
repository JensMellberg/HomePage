class IngredientPicker {
	static createPicker(categories, possibleIngredients, unitTypes, existing, callback) {
		const wrapper = HtmlUtils.createElement('div', 'category-picker');
		const wrapperInner = HtmlUtils.addElement('div', 'ingredient-picker-inner', wrapper);
		const wrapperInnerInner = HtmlUtils.addElement('div', 'ingredient-picker-inner-inner', wrapperInner);
		let allRows = []

		const addRow = (id, amount, unit, category) => {
			const rowWrapper = document.createElement('div')
			rowWrapper.className = 'input-wrapper'

			let instance = IngredientInstance.create(categories, possibleIngredients, unitTypes, id, amount, unit, category)
			allRows.push(instance)
			const elements = instance.createElements(false)
			const deleteButton = document.createElement('button')
			deleteButton.type = 'button'
			deleteButton.innerText = 'X'
			deleteButton.onclick = () => {
				allRows = allRows.filter(x => x != instance)
				rowWrapper.remove()
			}

			rowWrapper.appendChild(elements.searchBox)
			rowWrapper.appendChild(elements.categoryDropdown)
			rowWrapper.appendChild(elements.dropdown)

			if (unitTypes) {
				rowWrapper.appendChild(elements.amountBox)
				rowWrapper.appendChild(elements.unitDropdown)
			}
			
			rowWrapper.appendChild(deleteButton)

			wrapperInnerInner.appendChild(rowWrapper)
		}

		for (const ingredient of existing) {
			addRow(ingredient.id, ingredient.amount, ingredient.unit, ingredient.category)
		}

		const newButton = HtmlUtils.addElement('button', 'sectionButton', wrapperInner)
		newButton.type = 'button'
		newButton.innerText = 'Ny ingrediens';
		newButton.onclick = () => {
			addRow(null, 1, 'st', null)
		}

		const okButton = HtmlUtils.addElement('button', 'sectionButton', wrapper);
		okButton.type = 'button'
		okButton.innerText = 'Klar';
		$(okButton).click(() => {
			Popup.closePopup()
			callback(allRows.map(x => x.getIngredient()))
		});

		const quitButton = HtmlUtils.addElement('button', 'sectionButton', wrapper);
		quitButton.type = 'button'
		quitButton.innerText = 'Avbryt';
		$(quitButton).click(() => {
			Popup.closePopup()
		});

		Popup.putOnOverLay(wrapper, false);
	}
}