class IngredientInstance {

	static create(categories, possibleIngredients, unitTypes, ingredientId, amount, unit, category) {
		var instance = new IngredientInstance()
		instance.unitTypes = unitTypes
		instance.possibleIngredients = possibleIngredients
		instance.ingredientId = ingredientId
		instance.amount = amount || 1
		instance.unit = unit
		instance.categories = categories
		instance.category = category || possibleIngredients.filter(x => x.id === instance.ingredientId)[0]?.category || categories[0]
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
		if (this.amount.replaceAll) {
			this.amount = this.amount.replaceAll(',', '.')
		}

		const idHidden = HtmlUtils.createElement('input', '')
		idHidden.type = 'hidden'
		idHidden.value = this.ingredientId
		this.idHidden = idHidden

		const categoryHidden = HtmlUtils.createElement('input', '')
		categoryHidden.type = 'hidden'
		categoryHidden.value = this.category
		this.categoryHidden = categoryHidden

		const searchWrapper = HtmlUtils.createElement('div', 'ingredient-search-wrapper')
		const searchIcon = HtmlUtils.addElement('span', 'ingredient-search-icon', searchWrapper)
		searchIcon.innerText = '⌕'
		const searchInput = HtmlUtils.addElement('input', 'ingredient-search', searchWrapper)
		searchInput.type = 'text'
		searchInput.placeholder = 'Sök ingrediens..'

		const searchResultsWrapper = HtmlUtils.createElement('div', 'ingredient-search-results')
		
		const chooseCategoryButton = HtmlUtils.createElement('button', 'ingredient-category-button')
		chooseCategoryButton.type = 'button'
		const chooseCategoryText = HtmlUtils.addElement('span', '', chooseCategoryButton)
		chooseCategoryText.innerText = 'Välj kategori'
		const chooseCategoryArrow = HtmlUtils.addElement('span', 'ingredient-arrow', chooseCategoryButton)
		chooseCategoryArrow.innerText = '›'
		chooseCategoryButton.onclick = (e) => {
			$(ingredientListFromCategory).hide()
			$(categoryPicker).show()
			e.stopPropagation()
			$(document).one('click', () => {
				$(categoryPicker).hide()
				$(ingredientListFromCategory).hide()
			})
		}

		const ingredientListFromCategory = HtmlUtils.createElement('div', 'ingredient-list')
		$(ingredientListFromCategory).hide()

		const showIngredientListFromCategory = (categoryToShow) => {
			$(ingredientListFromCategory).show()
			$(ingredientListFromCategory).html('')
			for (const crntIngredient of this.possibleIngredients.filter(x => x.category === categoryToShow)) {
				const ingredientButton = HtmlUtils.addElement('button', 'ingredient-option', ingredientListFromCategory);
				ingredientButton.type = 'button'
				ingredientButton.innerText = crntIngredient.name
				ingredientButton.onclick = () => {
					updateIngredient(crntIngredient)
					$(categoryPicker).hide()
					$(ingredientListFromCategory).hide()
				}
			}
		}

		const categoryPicker = HtmlUtils.createElement('div', 'ingredient-category-picker')
		$(categoryPicker).hide()
		for (const crntCategory of this.categories) {
			const categoryButton = HtmlUtils.addElement('button', 'ingredient-category-option', categoryPicker)
			categoryButton.type = 'button'
			categoryButton.innerText = crntCategory
			categoryButton.onclick = (e) => {
				$(categoryPicker).hide()
				$(ingredientListFromCategory).hide()
				showIngredientListFromCategory(crntCategory)
				e.stopPropagation()
			}
		}

		const selectedInformationWrapper = HtmlUtils.createElement('div', 'ingredient-selected')
		$(selectedInformationWrapper).hide()
		const selectedIngredientName = HtmlUtils.addElement('div', 'ingredient-selected-name', selectedInformationWrapper)
		const selectedIngredientCategory = HtmlUtils.addElement('div', 'ingredient-category-badge', selectedInformationWrapper)

		const ingredientAmountRow = HtmlUtils.createElement('div', 'ingredient-amount-row')
		$(ingredientAmountRow).hide()
		const ingredientField = HtmlUtils.addElement('div', 'ingredient-field', ingredientAmountRow)
		const amountInfo = HtmlUtils.addElement('span', '', ingredientField)
		amountInfo.innerText = 'Mängd'

		const amountInput = HtmlUtils.addElement('input', '', ingredientField)
		amountInput.type = 'number'
		amountInput.step = 'any'
		this.amountInput = amountInput

		const ingredientFieldUnit = HtmlUtils.addElement('div', 'ingredient-field', ingredientAmountRow)
		const unitInfo = HtmlUtils.addElement('span', '', ingredientFieldUnit)
		unitInfo.innerText = 'Enhet'

		const unitDropdown = HtmlUtils.addElement('select', '', ingredientFieldUnit)
		this.unitDropdown = unitDropdown
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

		const updateIngredient = (ingredientToUse) => {
			this.idHidden.value = ingredientToUse.id
			this.categoryHidden.value = ingredientToUse.category
			selectedIngredientName.innerText = ingredientToUse.name
			selectedIngredientCategory.innerText = ingredientToUse.category
			updateUnitDropdown(ingredientToUse.unit, ingredientToUse.standardUnit)
			amountInput.value = ingredientToUse.standardAmount.replaceAll(',', '.')
			$(selectedInformationWrapper).show()
			$(ingredientAmountRow).show()
			amountInput.focus()
		}

		searchInput.oninput = () => {
			const searchString = searchInput.value.toLowerCase()
			if (!searchString) {
				$(searchResultsWrapper).toggleClass('visible', false)
				return
			}

			const hits = this.possibleIngredients.filter(
				x => x.name.toLowerCase().includes(searchString)
			);

			if (hits && hits.length === 1) {
				updateIngredient(hits[0])
			} else if (hits && hits.length > 0) {
				$(searchResultsWrapper).toggleClass('visible', true)
				$(searchResultsWrapper).html('')
				for (const result of hits) {
					const resultButton = HtmlUtils.addElement('button', 'ingredient-search-result', searchResultsWrapper)
					resultButton.type = 'button'
					const resultButtonText = HtmlUtils.addElement('span', 'ingredient-search-result-name', resultButton)
					resultButtonText.innerText = result.name
					const resultButtonCategory = HtmlUtils.addElement('span', 'ingredient-search-result-category', resultButton)
					resultButtonCategory.innerText = result.category
					resultButton.onclick = () => {
						updateIngredient(result)
						$(searchResultsWrapper).toggleClass('visible', false)
					}
				}
			}
		}

		if (this.ingredientId) {
			updateIngredient(this.possibleIngredients.filter(x => x.id == this.ingredientId)[0])
			unitDropdown.value = this.unit
			amountInput.value = this.amount
		}

		if (includeName) {
			idHidden.name = 'ingredientId'
			amountInput.name = 'amount'
			unitDropdown.name = 'unit'
		}

		return {
			searchWrapper,
			chooseCategoryButton,
			selectedInformationWrapper,
			ingredientAmountRow,
			searchResultsWrapper,
			idHidden,
			categoryHidden,
			categoryPicker,
			ingredientListFromCategory
		}
	}

	getIngredient() {
		return { id: this.idHidden.value, amount: this.amountInput.value.replace(',', '.'), unit: this.unitDropdown.value, category: this.categoryHidden.value  }
	}
}
