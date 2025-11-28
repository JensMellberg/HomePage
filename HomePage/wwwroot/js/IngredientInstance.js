class IngredientInstance {

	static create(categories, possibleIngredients, unitTypes, ingredientId, amount, unit, category) {
		var instance = new IngredientInstance()
		instance.unitTypes = unitTypes
		instance.possibleIngredients = possibleIngredients
		instance.ingredientId = ingredientId || possibleIngredients[0].id
		instance.amount = amount || 1
		instance.unit = unit
		instance.categories = categories
		instance.category = category || categories[0]
		return instance
	}

	static decodePossibleIngredients(encoded) {
		var list = []
		for (const val of encoded.split('¤')) {
			var tokens = val.split('|')
			list.push({ id: tokens[0], name: tokens[1], unit: tokens[2], category: tokens[3], standardAmount: tokens[4], standardUnit: tokens[5] })
		}

		return list
	}

	createElements(includeName) {
		const categoryDropdown = document.createElement('select')
		categoryDropdown.style.maxWidth = '60px'
		for (const category of this.categories) {
			const option = document.createElement('option');
			option.value = category;
			option.textContent = category;

			if (category === this.category) {
				option.selected = true;
			}

			categoryDropdown.appendChild(option);
		}

		const dropdown = document.createElement('select')
		let initialUnitType = 'Antal'

		const updateDropdownOptions = (cat, selectedId = null) => {
			dropdown.innerHTML = ''
			for (const ingredient of this.possibleIngredients.filter(x => x.category === cat)) {
				const option = document.createElement('option');
				option.value = ingredient.id;
				option.textContent = ingredient.name;

				if (ingredient.id === selectedId) {
					option.selected = true;
					initialUnitType = ingredient.unit
				}

				dropdown.appendChild(option);
			}

			dropdown.dispatchEvent(new Event('change'));
		}

		categoryDropdown.onchange = () => {
			const selected = categoryDropdown.value
			updateDropdownOptions(selected)
		}

		updateDropdownOptions(this.category, this.ingredientId)

		if (this.amount.replaceAll) {
			this.amount = this.amount.replaceAll(',', '.')
		}

		const amountBox = document.createElement('input')
		amountBox.required = true
		amountBox.type = 'number'
		amountBox.step = 'any'
		amountBox.value = this.amount.toString()
		amountBox.style.maxWidth = '40px'

		const unitDropdown = document.createElement('select')

		const updateUnitDropdown = (unitType, selectedUnit = null) => {
			unitDropdown.innerHTML = ''
			for (const type of (this.unitTypes?.[unitType] || [])) {
				var option = document.createElement('option');
				option.value = type;
				option.textContent = type;
				if (selectedUnit && type === selectedUnit) {
					option.selected = true;
				}

				unitDropdown.appendChild(option);
			}
		}

		updateUnitDropdown(initialUnitType, this.unit)

		dropdown.onchange = () => {
			const selected = this.possibleIngredients.find(x => x.id == dropdown.value)
			updateUnitDropdown(selected.unit, selected.standardUnit)
			amountBox.value = selected.standardAmount.replaceAll(',', '.')
		}

		if (includeName) {
			dropdown.name = 'ingredientId'
			amountBox.name = 'amount'
			unitDropdown.name = 'unit'
		}

		const searchBox = document.createElement('input')
		searchBox.type = 'text'
		searchBox.placeholder = 'Sök..'
		searchBox.style.maxWidth = '60px'
		searchBox.onchange = () => {
			const firstHit = this.possibleIngredients.find(
				x => x.name.toLowerCase().includes(searchBox.value.toLowerCase())
			);

			if (firstHit) {
				categoryDropdown.value = firstHit.category
				updateDropdownOptions(firstHit.category, firstHit.id)
			}
		}

		this.dropdown = dropdown
		this.amountBox = amountBox
		this.unitDropdown = unitDropdown
		this.categoryDropdown = categoryDropdown
		const deleteButton = document.createElement('button')
		deleteButton.type = 'button'
		deleteButton.innerText = 'X'
		return { categoryDropdown, dropdown, amountBox, unitDropdown, searchBox, deleteButton }
	}

	getIngredient() {
		return { id: this.dropdown.value, amount: this.amountBox.value.replace(',', '.'), unit: this.unitDropdown.value, category: this.categoryDropdown.value  }
	}
}
