class AutoSearch {
	static create(options, callback, wrapper, placeholder) {
		wrapper.className = 'auto-search-wrapper'
		const searchWrapperInner = HtmlUtils.addElement('div', 'auto-search-input-wrapper', wrapper)
		const searchIcon = HtmlUtils.addElement('span', 'auto-search-icon', searchWrapperInner)
		searchIcon.innerText = '⌕'
		const searchInput = HtmlUtils.addElement('input', 'auto-search', searchWrapperInner)
		searchInput.type = 'text'
		searchInput.placeholder = placeholder

		const searchResultsWrapper = HtmlUtils.addElement('div', 'auto-search-results', wrapper)

		searchInput.oninput = () => {
			const searchString = searchInput.value.toLowerCase()
			if (!searchString) {
				$(searchResultsWrapper).toggleClass('visible', false)
				return
			}

			const hits = options.filter(
				x => x.text.toLowerCase().includes(searchString)
			);

			if (hits && hits.length === 1) {
				callback(hits[0].id)
				$(searchResultsWrapper).toggleClass('visible', false)
			} else if (hits && hits.length > 0) {
				$(searchResultsWrapper).toggleClass('visible', true)
				$(searchResultsWrapper).html('')
				for (const result of hits) {
					const resultButton = HtmlUtils.addElement('button', 'auto-search-result', searchResultsWrapper)
					resultButton.type = 'button'
					const resultButtonText = HtmlUtils.addElement('span', 'auto-search-result-text', resultButton)
					resultButtonText.innerText = result.text
					resultButton.onclick = () => {
						callback(result.id)
						$(searchResultsWrapper).toggleClass('visible', false)
					}
				}
			}
		}

		document.onclick = () => {
			$(searchResultsWrapper).toggleClass('visible', false)
		}
    }
}
