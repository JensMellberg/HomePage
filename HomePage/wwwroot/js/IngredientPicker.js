class IngredientPicker {
	static createPicker(categories, possibleIngredients, unitTypes, existing, callback, newIngredientCallback) {
		const wrapper = HtmlUtils.createElement('div', 'ingredient-picker');
		const wrapperInner = HtmlUtils.addElement('div', 'ingredient-picker-list', wrapper);
		let allRows = []

		const newButton = HtmlUtils.addElement('button', 'modern-button secondary', wrapperInner)
		const addRow = (id, amount, unit, category) => {
			const rowWrapper = document.createElement('div')
			rowWrapper.className = 'ingredient-card'

			const header = HtmlUtils.addElement('div', 'ingredient-card-header', rowWrapper);
			const selector = HtmlUtils.addElement('div', 'ingredient-selector', header);

			let instance = IngredientInstance.create(categories, possibleIngredients, unitTypes, id, amount, unit, category)
			allRows.push(instance)
			const elements = instance.createElements(false)
			const deleteButton = document.createElement('button')
			deleteButton.className = 'ingredient-delete'
			deleteButton.type = 'button'
			deleteButton.innerText = 'X'
			deleteButton.onclick = () => {
				allRows = allRows.filter(x => x != instance)
				rowWrapper.remove()
			}

			selector.appendChild(elements.searchWrapper)
			selector.appendChild(elements.searchResultsWrapper)
			selector.appendChild(elements.chooseCategoryButton)
			selector.appendChild(elements.categoryPicker)
			selector.appendChild(elements.ingredientListFromCategory)
			header.appendChild(deleteButton)
			rowWrapper.appendChild(elements.selectedInformationWrapper)
			if (unitTypes) {
				rowWrapper.appendChild(elements.ingredientAmountRow)
			}

			/*if (unitTypes) {
				rowWrapper.appendChild(elements.amountBox)
				rowWrapper.appendChild(elements.unitDropdown)
			}*/

			//rowWrapper.appendChild(deleteButton)

			wrapperInner.insertBefore(rowWrapper, newButton)
		}

		newButton.type = 'button'
		newButton.innerText = 'Lägg till ingrediens';
		newButton.onclick = () => {
			addRow(null, 1, 'st', null)
		}

		for (const ingredient of existing) {
			addRow(ingredient.id, ingredient.amount, ingredient.unit, ingredient.category)
		}

		if (newIngredientCallback) {
			const newButton = HtmlUtils.addElement('button', 'modern-button secondary', wrapperInner)
			newButton.type = 'button'
			newButton.innerText = 'Skapa ny ingrediens';
			newButton.onclick = () => {
				newIngredientCallback(allRows.map(x => x.getIngredient()))
			}
		}

		const okButton = HtmlUtils.addElement('button', 'modern-button primary', wrapper);
		okButton.style.marginRight = '10px'
		okButton.type = 'button'
		okButton.innerText = 'Klar';
		$(okButton).click(() => {
			const updatedIngredients = allRows
				.map(x => x.getIngredient())
				.filter(x => x.id)

			Popup.closePopup()
			callback(updatedIngredients)
		});

		const quitButton = HtmlUtils.addElement('button', 'modern-button secondary', wrapper);
		quitButton.type = 'button'
		quitButton.innerText = 'Avbryt';
		$(quitButton).click(() => {
			Popup.closePopup()
		});

		Popup.putOnOverLay(wrapper, false);
	}
}